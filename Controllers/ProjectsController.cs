using Htmx;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Project;
using ProjectManagement.App.DTO.Workspace;
using ProjectManagement.App.Extensions;
using ProjectManagement.App.Filters;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Repository;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.Services.Interfaces;
using ProjectManagement.App.ViewModel.Project;
using ProjectManagement.App.ViewModel.Workspace;
using System.Security.Claims;

namespace ProjectManagement.App.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IProjectRepository _projectRepository; 
        private readonly IGithubService _githubService;
        private readonly IGithubRepository _githubRepo;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IWorkspaceRepository _workspaceRepository;

        public ProjectsController(IProjectRepository projectRepository, 
            IGithubService githubService, 
            IGithubRepository githubRepo,
            IProjectMemberRepository projectMemberRepository,
            IAuthRepository authRepository,
            IWorkspaceRepository workspaceRepository)
        {
            _projectRepository = projectRepository;
            _githubService = githubService;
            _githubRepo = githubRepo;
            _projectMemberRepository = projectMemberRepository;
            _authRepository = authRepository;
            _workspaceRepository = workspaceRepository;
        }

        [ProjectAuthorize]
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null) return RedirectToAction("Index", "Home");

            var projectData = await _projectRepository.GetAsync(id, workspaceId.Value, userId!);

            if (projectData == null) return NotFound();

            var project = ConvertToProjectViewModel(projectData);

            // Ambil Team Members dari repository khusus
            var members = await _projectMemberRepository.GetProjectMembersAsync(id);

            var data = new ProjectDetailsDto()
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Status = project.Status.ToString(),
                DueDate = project.DueDate,
                IsConnected = project.IsConnected,
                Progress = project.Progress,
                TotalTasks = project.TotalTasks,
                CompletedTasks = project.CompletedTasks,
                TotalCommits = projectData.GithubRepoConnecteds.FirstOrDefault(i=> i.Connected)?.Repo?.Commits?.Count() ?? 0,
                Members = members.Select(m => new ProjectMemberDto
                {
                    UserId = m.UserId,
                    Name = m.User.UserName ?? m.User.Email ?? "Unknown",
                    Role = m.Role.ToString()
                }).ToArray(),
                Tasks = projectData.Tasks.Select(t => new ProjectTaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status.ToString(),
                    DueDate = t.TargetDate,
                    Commits = t.Commits.Count()
                }).OrderBy(i => i.Status == "InProgress" ? 0 : i.Status == "ToDo" ? 1 : 2)
                       .ThenByDescending(i => i.DueDate).ToArray()
            };

            // Menyiapkan data untuk Dialog Add Member (Hanya user di workspace yang sama)
            var workspaceUsers = await _workspaceRepository.GetWorkspaceMembersAsync(workspaceId.Value);
            ViewBag.AllUsers = workspaceUsers.Select(u => new { value = u.Id, text = u.UserName ?? u.Email }).ToList();

            ViewBag.Roles = Enum.GetValues(typeof(ProjectRole))
                .Cast<ProjectRole>()
                .Select(r => new { value = (int)r, text = r.ToString() })
                .ToList();

            var currentUserMember = members.FirstOrDefault(m => m.UserId == userId);
            var currentUserRole = currentUserMember?.Role;
            if (projectData.ProjectOwnerUserId == userId)
            {
                currentUserRole = ProjectRole.Owner;
            }
            ViewBag.CurrentUserRole = currentUserRole;
            ViewBag.CurrentUserId = userId;

            var checkConnectProject = await _projectRepository.CheckConnectedProject(project.Id, workspaceId.Value, userId!);

            if (checkConnectProject.Success && User.IsConnectedGithub())
            {
                await SynchronizeCommitAsync(checkConnectProject);
            }


            if (Request.IsHtmx())
            {
                Response.Htmx(h =>
                {
                    h.PushUrl(Request.GetEncodedUrl());
                });

                return PartialView(data);
            }


            return View(data);
        }

        [HttpPost]
        [ProjectAuthorize(ProjectRole.Owner)]
        public async Task<IActionResult> AddMember(AddProjectMemberDto model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data." });
            }

            var success = await _projectMemberRepository.AddMemberAsync(model.ProjectId, model.UserId, model.Role);
            
            if (success)
            {
                return Json(new { success = true, message = "Member added successfully." });
            }

            return Json(new { success = false, message = "User is already a member or failed to add." });
        }

        [HttpPost]
        [ProjectAuthorize(ProjectRole.Owner, ProjectRole.Manager, ProjectRole.TeamLead)]
        public async Task<IActionResult> UpdateMemberRole(AddProjectMemberDto model)
        {
            var workspaceId = User.GetWorkspaceId();
            if (workspaceId == null) return Json(new { success = false, message = "Workspace context not found." });

            var success = await _projectMemberRepository.UpdateMemberRoleAsync(model.ProjectId, model.UserId, model.Role);
            if (success)
            {
                return Json(new { success = true, message = "Member role updated." });
            }
            return Json(new { success = false, message = "Failed to update member role." });
        }

        [HttpPost]
        [ProjectAuthorize(ProjectRole.Owner, ProjectRole.Manager)]
        public async Task<IActionResult> RemoveMember(int projectId, string userId)
        {
            var workspaceId = User.GetWorkspaceId();
            if (workspaceId == null) return Json(new { success = false, message = "Workspace context not found." });

            var success = await _projectMemberRepository.RemoveMemberAsync(projectId, userId);
            if (success)
            {
                return Json(new { success = true, message = "Member removed." });
            }
            return Json(new { success = false, message = "Failed to remove member. (Note: Owner cannot be removed)" });
        }

        public async Task<IActionResult> Page(int page = 1, int pageSize = 12)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null) return RedirectToAction("Index", "Home");

            var allProject = await _projectRepository.GetAllAsync(workspaceId.Value, userId!);

            var totalRecords = allProject.Count();

            var dataList = allProject.ToList();

            // Trigger sync for connected projects on page load
            if (User.IsConnectedGithub())
            {
                foreach (var p in dataList.Where(p => p.GithubRepoConnecteds.Any(c => c.Connected)))
                {
                    var checkConnectProject = await _projectRepository.CheckConnectedProject(p.Id, workspaceId.Value, userId!);
                    if (checkConnectProject.Success)
                    {
                        await SynchronizeCommitAsync(checkConnectProject);
                    }
                }
            }

            var data = dataList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(i => i.CreatedAt)
                .Select(ConvertToProjectViewModel);

            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;


            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> GetPagedProjects(int currentPage, int pageSize)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var workspaceId = User.GetWorkspaceId();

            if (workspaceId == null) return PartialView("_ProjectCardListPartial", new List<ProjectCardViewModel>());

            var allProject = await _projectRepository.GetAllAsync(workspaceId.Value, userId!);


            var data = allProject
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .Select(ConvertToProjectViewModel).ToList();

            return PartialView("_ProjectCardListPartial", data);
        }

        private static ProjectCardViewModel ConvertToProjectViewModel(Models.Workspace.Project i)
        {
            return new ProjectCardViewModel()
            {
                Id = i.Id,
                Title = i.Name,
                Description = i.Description,
                Status = i.Tasks.Count != 0 ? i.Tasks.FirstOrDefault().Status : Models.Enum.Status.ToDo,
                TotalTasks = i.Tasks.Count,
                CompletedTasks = i.Tasks.Count != 0 ? i.Tasks.Where(i => i.Status == Models.Enum.Status.Done).Count() : 0,
                DueDate = i.EndDate,
                IsConnected = i.GithubRepoConnecteds.Any(i => i.Connected)
            };
        }

        //public async Task<IActionResult> Details(int id)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    var projectData = await _projectRepository.GetAsync(id,userId);

        //    var project = ConvertToProjectViewModel(projectData);


        //    var data = new ProjectDetailsDto()
        //    {
        //        Id = project.Id,
        //        Title = project.Title,
        //        Description = project.Description,
        //        Status = project.Status.ToString(),
        //        DueDate = project.DueDate,
        //        IsConnected = project.IsConnected,
        //        Progress = project.Progress,
        //        TotalTasks = project.TotalTasks,
        //        CompletedTasks = project.CompletedTasks,
        //        TotalCommits = projectData.Tasks.Sum(i => i.Commits.Count()),
        //        Members = new string[] { "Test", "Agus" },
        //        Tasks = projectData.Tasks.Any() ? projectData.Tasks.Select(i => new ProjectTaskDto()
        //        {
        //            Id = i.Id,
        //            Title = i.Title,
        //            Description = i.Description,
        //            Commits = i.Commits.Count(),
        //            DueDate = i.TargetDate,
        //            Status = i.Status.ToString()
        //        })
        //        .OrderBy(i => i.Status == "InProgress" ? 0 : i.Status == "ToDo" ? 1 : 2)
        //        .ThenByDescending(i => i.DueDate)
                    
        //        .ToArray() : Array.Empty<ProjectTaskDto>()

        //    };

        //    var checkConnectProject = await _projectRepository.CheckConnectedProject(project.Id);

        //    if (checkConnectProject.Success && User.IsConnectedGithub())
        //    {
        //        await SynchronizeCommitAsync(checkConnectProject);
        //    }


        //    if (Request.IsHtmx())
        //    {
        //        Response.Htmx(h =>
        //        {
        //            h.PushUrl(Request.GetEncodedUrl());
        //        });

        //        return PartialView(data);
        //    }

        //        return View(data);
        //}

        private async Task SynchronizeCommitAsync(ResponseResultDto<GitHubRepoDto> checkConnectProject)
        {
            var repoData = checkConnectProject.Data;


            if (!User.IsConnectedGithub())
            {
                var errMessage = "User github credential is expired or not found";

                throw new Exception(errMessage);
            }

            var accesToken = User.GetGithubToken();

            repoData.RepoOwner = User.GetGithubUsername();

            var commitResponse = await _githubService.GetCommitsAsync(repoData, accesToken);

            if (commitResponse.Success)
            {
                var commitData = commitResponse.Data;

                var existingShas = repoData.Commits.Select(c => c.Sha).ToHashSet();

                var newCommits = commitData.Where(c => !existingShas.Contains(c.Sha)).ToList();

                if (newCommits.Any())
                {
                    var repoResponse = await _githubRepo.InsertGithubCommit(newCommits, repoData.RepoId);

                    if (repoResponse.Success)
                    {
                        TempData["RepoNotification"] = $"Synchronized {newCommits.Count} new commits for {repoData.Name}";
                    }
                    else
                    {
                        TempData["RepoNotificationFailed"] = $"Failed to save new commits for {repoData.Name}";
                    }
                }
                // Optional: set a message even if no new commits, but maybe too noisy for the list page.
                // For now, we only set it if there are new ones or an error occurred.
            }
        }
    }
}
