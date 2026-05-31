using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.App.Data;
using ProjectManagement.App.DTO;
using ProjectManagement.App.Models;
using ProjectManagement.App.Models.Workspace;
using ProjectManagement.App.Repository.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectManagement.App.Repository
{
    public class InviteRepository : IInviteRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;


        public InviteRepository(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<ResponseResultDto<ProjectManagement.App.Models.Workspace.WorkspaceInvite>> CreateInviteAsync(int workspaceId, string inviterUserId, string inviteeEmail)
        {
            // 1. Check if user exists by email
            var userToInvite = await _userManager.FindByEmailAsync(inviteeEmail);
            if (userToInvite != null)
            {
                // 2. Check if user is already in the workspace
                var isAlreadyMember = await _dbContext.WorkspaceMembers
                    .AnyAsync(wm => wm.WorkspaceId == workspaceId && wm.UserId == userToInvite.Id);
                if (isAlreadyMember)
                {
                    return new ResponseResultDto<ProjectManagement.App.Models.Workspace.WorkspaceInvite> { Success = false, Message = "User is already a member of this workspace." };
                }
            }

            // 3. Check for an existing, pending invite
            var existingInvite = await _dbContext.WorkspaceInvites
                .FirstOrDefaultAsync(i => i.WorkspaceId == workspaceId && i.InviteeEmail == inviteeEmail && i.Status == InviteStatus.Pending);

            if (existingInvite != null)
            {
                // Optional: Resend notification instead of creating a new one
                return new ResponseResultDto<ProjectManagement.App.Models.Workspace.WorkspaceInvite> { Success = true, Message = "An invitation has already been sent to this email address." };
            }

            // 4. Create new invite
            var invite = new WorkspaceInvite
            {
                WorkspaceId = workspaceId,
                InviterUserId = inviterUserId,
                InviteeEmail = inviteeEmail,
                Token = Guid.NewGuid().ToString("N"), // Secure, random token
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                Status = InviteStatus.Pending
            };

            await _dbContext.WorkspaceInvites.AddAsync(invite);
            await _dbContext.SaveChangesAsync();

            return new ResponseResultDto<ProjectManagement.App.Models.Workspace.WorkspaceInvite> { Success = true, Message = "Invitation created successfully.", Data = invite };
        }

        public async Task<ResponseResultDto> AcceptInviteAsync(string token, string inviteeUserId)
        {
            var invite = await _dbContext.WorkspaceInvites
                .FirstOrDefaultAsync(i => i.Token == token && i.Status == InviteStatus.Pending);

            if (invite == null)
            {
                return new ResponseResultDto { Success = false, Message = "Invitation not found or is no longer valid." };
            }

            if (invite.ExpiryDate < DateTime.UtcNow)
            {
                invite.Status = InviteStatus.Expired;
                await _dbContext.SaveChangesAsync();
                return new ResponseResultDto { Success = false, Message = "This invitation has expired." };
            }

            var user = await _dbContext.Users.FindAsync(inviteeUserId);
            if (user == null || !user.Email.Equals(invite.InviteeEmail, StringComparison.OrdinalIgnoreCase))
            {
                return new ResponseResultDto { Success = false, Message = "This invitation is for a different user." };
            }

            // Add user to workspace
            var newMember = new WorkspaceMember
            {
                WorkspaceId = invite.WorkspaceId,
                UserId = inviteeUserId,
                Role = WorkspaceRole.Member
            };

            await _dbContext.WorkspaceMembers.AddAsync(newMember);
            
            // Update invite status
            invite.Status = InviteStatus.Accepted;
            invite.InviteeId = inviteeUserId;

            // Mark related notifications as read
            var notifications = await _dbContext.Notifications
                .Where(n => n.RelatedInviteId == invite.Id && n.RecipientUserId == inviteeUserId && !n.IsRead)
                .ToListAsync();
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _dbContext.SaveChangesAsync();

            return new ResponseResultDto { Success = true, Message = "Successfully joined the workspace." };
        }

        public async Task<ResponseResultDto> DeclineInviteAsync(string token, string inviteeUserId)
        {
            var invite = await _dbContext.WorkspaceInvites
                .FirstOrDefaultAsync(i => i.Token == token && i.Status == InviteStatus.Pending);
            
            if (invite == null)
            {
                return new ResponseResultDto { Success = false, Message = "Invitation not found or is no longer valid." };
            }

            var user = await _dbContext.Users.FindAsync(inviteeUserId);
            if (user == null || !user.Email.Equals(invite.InviteeEmail, StringComparison.OrdinalIgnoreCase))
            {
                return new ResponseResultDto { Success = false, Message = "This invitation is for a different user." };
            }

            invite.Status = InviteStatus.Declined;

            // Mark related notifications as read
            var declineNotifications = await _dbContext.Notifications
                .Where(n => n.RelatedInviteId == invite.Id && n.RecipientUserId == inviteeUserId && !n.IsRead)
                .ToListAsync();
            foreach (var notification in declineNotifications)
            {
                notification.IsRead = true;
            }

            await _dbContext.SaveChangesAsync();

            return new ResponseResultDto { Success = true, Message = "Invitation declined." };
        }


        public async Task AcceptPendingInvitesForUser(string userEmail, string userId)
        {
            var pendingInvites = await _dbContext.WorkspaceInvites
                .Where(i => i.InviteeEmail.Equals(userEmail) && i.Status == InviteStatus.Pending)
                .ToListAsync();

            if (!pendingInvites.Any()) return;

            foreach (var invite in pendingInvites)
            {
                var isAlreadyMember = await _dbContext.WorkspaceMembers
                    .AnyAsync(wm => wm.WorkspaceId == invite.WorkspaceId && wm.UserId == userId);
                
                if (!isAlreadyMember)
                {
                    var newMember = new WorkspaceMember
                    {
                        WorkspaceId = invite.WorkspaceId,
                        UserId = userId,
                        Role = WorkspaceRole.Member
                    };
                    await _dbContext.WorkspaceMembers.AddAsync(newMember);
                }

                invite.Status = InviteStatus.Accepted;
                invite.InviteeId = userId;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
