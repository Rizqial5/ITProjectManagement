using Htmx;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.App.DTO;
using ProjectManagement.App.DTO.Task;
using ProjectManagement.App.Models.Enum;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository;
using ProjectManagement.App.Repository.Interface;
using ProjectManagement.App.ViewModel;
using Syncfusion.EJ2.Base;
using Ganss.Xss;
using ProjectManagement.App.Filters;
using System.Security.Claims;


namespace ProjectManagement.App.Controllers
{
    public class TaskController : Controller
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly IProjectRepository _projectRepository;

        public TaskController(ITaskRepository taskRepository, IProjectMemberRepository projectMemberRepository, IProjectRepository projectRepository)
        {
            _taskRepository = taskRepository;
            _projectMemberRepository = projectMemberRepository;
            _projectRepository = projectRepository;
        }

        [Route("task/{projectId:int}/{taskId:int}")]
        [ProjectAuthorize]
        public async Task<IActionResult> Details(int projectId, int taskId, bool isConnected)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var taskItem = await _taskRepository.GetAsync(projectId, taskId, userId);
            var countCommit = isConnected ? await _taskRepository.GetTotalIntegratedCommit(projectId, taskId, userId) : 0;

            if(taskItem == null)
            {
                TempData["RepoNotificationFailed"] = "Task is not exists or access denied";
                return RedirectToAction("Index", "Workspace", new { ProjectID = projectId });
            }

            var members = await _projectMemberRepository.GetProjectMembersAsync(projectId);

            string? repoUrl = null;
            if (isConnected)
            {
                var checkRepo = await _projectRepository.CheckConnectedProject(projectId, userId);
                if (checkRepo.Success && checkRepo.Data != null)
                {
                    repoUrl = checkRepo.Data.Html_Url;
                }
            }

            var taskModel = new TaskViewModel()
            {
                TaskId = taskItem.Id,
                Title = taskItem.Title,
                ProjectName = taskItem.Project.Name,
                ProjectUrl = Url.Action("Details", "Projects", new { id = projectId }),
                ProjectId = taskItem.Project.Id,
                Description = taskItem.Description,
                Status = taskItem.Status.ToString(),
                DueDate = taskItem.TargetDate,
                TotalLinkedCommits = countCommit,
                isConnectedRepo = isConnected,
                Commits = taskItem.Commits.ToList(),
                isRequestHtmx = Request.IsHtmx(),
                LastUpdated = taskItem.UpdatedAt.ToString("dd-MMM-yyyy"),
                AssigneeName = taskItem.AssignedUser?.UserName,
                AssignedUserId = taskItem.AssignedUserId,
                GithubRepoUrl = repoUrl,
                AvailableAssignees = members.Select(m => new DropdownItem
                {
                    value = m.UserId,
                    text = m.User.UserName ?? m.User.Email ?? "Unknown"
                }).ToList()


            };

            if(Request.IsHtmx())
            {
                Response.Htmx(h =>
                {
                    h.PushUrl(Request.GetEncodedUrl());
                });

                return PartialView(taskModel);
            }

            return View(taskModel);
        }


        [HttpGet]
        [ProjectAuthorize]
        public IActionResult ShowLinkCommit(int projectId, bool isConnected)
        {
      
            var model = new TaskViewModel
            {
                ProjectId = projectId,
                isConnectedRepo = isConnected
            };


            return PartialView("_LinkCommitGrid", model);

        }

        [HttpGet]
        [ProjectAuthorize(ProjectRole.Owner, ProjectRole.Manager)]
        public async Task<IActionResult> ShowUpdateDialog(int projectId, int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var taskItem = await _taskRepository.GetAsync(projectId, taskId, userId);
            if (taskItem == null) return NotFound();

            var model = new UpdateDateDto
            {
                ProjectId = projectId,
                TaskId = taskId,
                SetNewDate = taskItem.TargetDate,
                ControllerName = "Task"
            };


            return PartialView("_UpdateDateDialog", model);

        }

        [HttpGet]
        [ProjectAuthorize]
        public async Task<IActionResult> ShowLinkedCommit(int projectId, int repoId, int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = await _taskRepository.GetLinkedCommit(repoId, taskId, userId);


            return PartialView("_ListLinkedCommit", model);

        }

        [HttpGet]
        [ProjectAuthorize]
        public async Task<IActionResult> EditTaskDetails(int projectId,int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _taskRepository.GetAsync(projectId, taskId, userId); 
            if (task == null) return NotFound();

            var members = await _projectMemberRepository.GetProjectMembersAsync(projectId);

            var model = new EditTaskDescDto
            {
                ProjectId = projectId,
                TaskId = task.Id,
                Description = task.Description,
                Status = task.Status.ToString(),
                AssignedUserId = task.AssignedUserId,
                LastUpdated = task.UpdatedAt.ToString("dd-MMM-yyyy")
            };

            ViewBag.AvailableAssignees = members.Select(m => new 
            {
                value = m.UserId,
                text = m.User.UserName ?? m.User.Email ?? "Unknown"
            }).ToList();

            return PartialView("_EditTaskDescPanel", model);
        }

        [HttpPost]
        [ProjectAuthorize]
        public async Task<IActionResult> UpdateTaskDetails(EditTaskDescDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {

                TaskItem updatedData = new()
                {
                    Id = model.TaskId,
                    Description = model.Description,
                    Status = Enum.Parse<Status>(model.Status.Trim()),
                    AssignedUserId = model.AssignedUserId
                };

                var success = await _taskRepository.UpdateAsync(model.ProjectId, updatedData, userId);

                if(success)
                {
                    var task = await _taskRepository.GetAsync(model.ProjectId, model.TaskId, userId);

                    var updatedModel = new TaskViewModel
                    {
                        ProjectId = model.ProjectId,
                        TaskId = task.Id,
                        Description = task.Description,
                        Status = task.Status.ToString(),
                        AssigneeName = task.AssignedUser?.UserName,
                        AssignedUserId = task.AssignedUserId,
                        LastUpdated = task.UpdatedAt.ToString("dd-MMM-yyyy"),
                        isRequestHtmx = Request.IsHtmx(),
                        InitPage = false
                    };
                    return PartialView("_TaskDescPanel", updatedModel);

                    
                }
                else
                {
                    return Json(new { success = false, message = "Update failed or unauthorized." });
                }

            }

            return PartialView("_EditTaskDescPanel", model);
        }

        [HttpPost]
        [ProjectAuthorize(ProjectRole.Owner, ProjectRole.Manager)]
        public async Task<IActionResult> UpdateDate(UpdateDateDto model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ModelState.IsValid)
            {
                TaskItem updatedData = new()
                {
                    Id = model.TaskId,
                    ProjectId = model.ProjectId,
                    TargetDate = model.SetNewDate,
                };

                var success = await _taskRepository.UpdateDateAsync(updatedData, userId);

                if (success)
                {
                    if (Request.IsHtmx())
                    {
                        var redirectUrl = Url.Action("Details", new { projectId = model.ProjectId, taskId = model.TaskId, isConnected = true });
                        return Json(new { success = true, redirectUrl });
                    }
                    return RedirectToAction("Details", new { projectId = model.ProjectId, taskId = model.TaskId, isConnected = true });
                }
                else
                {
                    // Update failed, return error for HTMX
                    if (Request.IsHtmx())
                    {
                        return Json(new { success = false, errorMessage = "Failed to update the date. Please try again." });
                    }
                }
            }
            else
            {
                // Validation failed, return error for HTMX
                if (Request.IsHtmx())
                {
                    // You can aggregate ModelState errors if needed
                    var errorMsg = string.Join("; ", ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage));
                    return Json(new { success = false, errorMessage = errorMsg });
                }
            }

            // Fallback: return partial for non-HTMX or if you want to re-render the form
            if (Request.IsHtmx())
            {
                return PartialView("_UpdateDateDialog", model);
            }
            return View(model);
        }


        [HttpGet]
        [ProjectAuthorize]
        public async Task<IActionResult> DetailsPanel(int projectId, int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var task = await _taskRepository.GetAsync(projectId, taskId, userId);
            if (task == null) return NotFound();
            var model = new TaskViewModel
            {
                ProjectId = projectId,
                TaskId = task.Id,
                Description = task.Description,
                Status = task.Status.ToString(),
                LastUpdated = task.UpdatedAt.ToString("dd-MMM-yyyy"),
                isRequestHtmx = Request.IsHtmx(),
                InitPage = false
            };
            return PartialView("_TaskDescPanel", model);
        }


        [HttpPost]
        [ProjectAuthorize]
        public async Task<IActionResult> GetCommitRepo([FromBody] DataManagerRequest DataManagerRequest, int projectId, int taskId, bool isConnected)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IEnumerable<CommitDto> commitData;


            if (isConnected)
            {
                commitData = await _taskRepository.GetAllIntegratedCommitAsync(projectId, taskId, userId);

                
            }
            else
            {
                commitData = new List<CommitDto>(0);
            }

            DataOperations dataOperations = new();
            var result = dataOperations.Execute(commitData, DataManagerRequest);


            return Json(new { result = result, count = commitData.Count()});

        }

        [HttpPost]
        [ProjectAuthorize]
        public async Task<IActionResult> GetAvailableCommit([FromBody] DataManagerRequest DataManagerRequest, int projectId, bool isConnected)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            IEnumerable<CommitDto> commitData;


            if (isConnected)
            {
                commitData = await _taskRepository.GetAllCommitAsync(projectId, userId);


            }
            else
            {
                commitData = new List<CommitDto>(0);
            }

            DataOperations dataOperations = new();
            var result = dataOperations.Execute(commitData, DataManagerRequest);

            // Proyeksi data untuk menyederhanakan tampilan di Grid
            var listResult = (result as IEnumerable<CommitDto>)?.Select(c => new
            {
                c.Id,
                c.Message,
                c.AuthorName,
                c.CommitDate,
                c.IsIntegrated,
                c.Sha,
                c.RepoId,
                ShortSha = string.IsNullOrEmpty(c.Sha) ? "" : (c.Sha.Length > 7 ? c.Sha.Substring(0, 7) : c.Sha),
                AuthorInitial = string.IsNullOrEmpty(c.AuthorName) ? "?" : c.AuthorName.Substring(0, 1).ToUpper()
            });

            return Json(new { result = listResult, count = commitData.Count() });

        }

        [HttpPost]
        [ProjectAuthorize]
        public async Task<IActionResult> LinkCommit(int projectId, int commitId, int repoId, int taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var result = await _taskRepository.ConnectCommitToTaskAsync(repoId, commitId, taskId, userId);

                if (result.Success)
                {
                    TempData["RepoNotification"] = "Commit has been sucessfully linked";
                }
                else
                {
                    TempData["RepoNotificationFailed"] = result.Message;
                }

                return Json(new { success = result.Success });
            }
            catch(Exception ex)
            {
                TempData["RepoNotificationFailed"] = "Error when linking commit";
                return Json(new { success = false, message = ex.Message });
            }


        }


        [HttpPost]
        [ProjectAuthorize]
        public async Task<IActionResult> SetToDone(int taskId, int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                var status = Status.Done;

                var result = await _taskRepository.SetTaskStatus(projectId, taskId, status, userId);

                if (result.Success)
                {
                    TempData["RepoNotification"] = result.Message;
                    if (Request.IsHtmx())
                    {
                        // Tell HTMX to redirect/refresh the page
                        Response.Htmx(h => h.Redirect(Url.Action("Details", new { projectId, taskId, isConnected = true })));
                        return Ok();
                    }
                }
                else
                {
                    TempData["RepoNotificationFailed"] = result.Message;
                }

                return Json(new { success = result.Success, message = result.Message });
            }
            catch (Exception ex)
            {
                TempData["RepoNotificationFailed"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        [ProjectAuthorize(ProjectRole.Owner, ProjectRole.Manager)]
        public async Task<IActionResult> AddTask(CreateTaskDto newTask, int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _taskRepository.AddAsync(projectId, newTask, userId);

                TempData["RepoNotification"] = $"Task {newTask.Title} added succesfully";

                return Json(new { success = true });
            }
            catch(Exception ex)
            {
                TempData["RepoNotificationFailed"] = ex.Message;

                return Json(new { success = false });
            }

        }

        [HttpPost]
        [ProjectAuthorize]
        public async Task<IActionResult> AddNotes([FromBody] SaveNotesDto dataNote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _taskRepository.AddNoteAsync(dataNote, userId);

                return Json(new { success = true, message= "Notes sucessfully saved" });
            }
            catch (Exception ex)
            {


                return Json(new { success = false, message = ex.Message });
            }

        }

    }
}
