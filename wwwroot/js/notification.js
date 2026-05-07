// notification.js

document.addEventListener('DOMContentLoaded', function () {
    const notificationBell = document.getElementById('notificationBell');
    if (!notificationBell) return; // Exit if notification UI is not present

    const notificationCountBadge = document.getElementById('notificationCount');
    const notificationDropdownContent = document.getElementById('notificationDropdownContent');
    const markAllAsReadBtn = document.getElementById('markAllAsReadBtn');
    let notificationPollingInterval;

    // Function to fetch and display notifications
    async function loadNotifications() {
        try {
            const response = await fetch('/Notifications/unread');
            if (!response.ok) {
                if (response.status === 401) {
                    console.warn("User not authenticated for notifications. Stopping polling.");
                    clearInterval(notificationPollingInterval);
                    return;
                }
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            const notifications = await response.json();

            if (notificationDropdownContent) {
                notificationDropdownContent.innerHTML = ''; // Clear previous
            }
            
            const notificationCount = notifications.length;

            if (notificationCountBadge) {
                notificationCountBadge.textContent = notificationCount > 0 ? notificationCount : '';
                notificationCountBadge.style.display = notificationCount > 0 ? 'flex' : 'none';
            }

            if (notificationCount === 0) {
                if (notificationDropdownContent) {
                    notificationDropdownContent.innerHTML = '<div class="dropdown-item text-center text-secondary small py-3">No new notifications</div>';
                }
            } else {
                notifications.forEach(notification => {
                    const item = document.createElement('div'); // Use div as container
                    item.className = 'dropdown-item d-flex align-items-start py-2';

                    let actionButtons = '';
                    if (notification.Type === "WorkspaceInvite" && notification.ActionUrl) {
                        actionButtons = `
                            <div class="mt-2">
                                <a href="${notification.ActionUrl}" class="btn btn-sm btn-primary px-2 py-1 me-1">Accept</a>
                                <a href="${notification.DeclineUrl || '#'}" class="btn btn-sm btn-light px-2 py-1">Decline</a>
                            </div>
                        `;
                    }

                    item.innerHTML = `
                        <div class="icon-circle bg-primary text-white me-3" style="width: 2.5rem; height: 2.5rem; flex-shrink: 0;">
                            <i class="${notification.IconCssClass || 'fas fa-bell'}"></i>
                        </div>
                        <div>
                            <strong class="text-main">${notification.Title}</strong>
                            <div class="text-truncate" style="max-width: 250px;">${notification.Message}</div>
                            <div class="small text-gray-500">${timeAgo(notification.CreatedAt)}</div>
                            ${actionButtons}
                        </div>
                    `;

                    // Mark as read only if it's NOT an actionable invite, or handle after action
                    if (notification.Type !== "WorkspaceInvite") {
                         item.addEventListener('click', async function (event) {
                            if (notification.ActionUrl && notification.ActionUrl !== '#') {
                                event.preventDefault();
                                await markNotificationAsRead(notification.Id);
                                window.location.href = notification.ActionUrl;
                            }
                        });
                    }

                    if (notificationDropdownContent) {
                        notificationDropdownContent.appendChild(item);
                    }
                });
            }
        } catch (error) {
            console.error("Error loading notifications:", error);
            if (notificationCountBadge) {
                notificationCountBadge.style.display = 'none';
            }
            if (notificationDropdownContent) {
                notificationDropdownContent.innerHTML = '<div class="dropdown-item text-center text-danger small py-3">Failed to load notifications</div>';
            }
        }
    }

    async function markNotificationAsRead(notificationId) {
        try {
            await fetch(`/Notifications/mark-as-read/${notificationId}`, {
                method: 'POST'
            });
            // Immediately reduce count visually and then reload for consistency
            const currentCount = parseInt(notificationCountBadge.textContent || '0');
            if (currentCount > 1) {
                notificationCountBadge.textContent = currentCount - 1;
            } else {
                notificationCountBadge.style.display = 'none';
            }
        } catch (error) {
            console.error("Error marking notification as read:", error);
        }
    }

    function timeAgo(dateString) {
        const now = new Date();
        const past = new Date(dateString);
        const seconds = Math.floor((now - past) / 1000);

        let interval = seconds / 31536000;
        if (interval > 1) return Math.floor(interval) + " years ago";
        interval = seconds / 2592000;
        if (interval > 1) return Math.floor(interval) + " months ago";
        interval = seconds / 86400;
        if (interval > 1) return Math.floor(interval) + " days ago";
        interval = seconds / 3600;
        if (interval > 1) return Math.floor(interval) + " hours ago";
        interval = seconds / 60;
        if (interval > 1) return Math.floor(interval) + " minutes ago";
        return Math.floor(seconds) + " seconds ago";
    }

    // Initial and periodic loading
    loadNotifications();
    notificationPollingInterval = setInterval(loadNotifications, 30000); // 30 seconds

    // Event listener for the "Mark all as read" button
    if (markAllAsReadBtn) {
        markAllAsReadBtn.addEventListener('click', async function() {
            try {
                await fetch('/Notifications/mark-all-as-read', {
                    method: 'POST'
                });
                loadNotifications(); // Reload to clear all
                ejs.notifications.ToastUtility.show("All notifications marked as read!", 'Success', 3000);
            } catch (error) {
                console.error("Error marking all notifications as read:", error);
                ejs.notifications.ToastUtility.show("Failed to mark all as read.", 'Error', 3000);
            }
        });
    }

    // Refresh notifications when dropdown is opened for better UX
    notificationBell.closest('.dropdown')?.addEventListener('show.bs.dropdown', loadNotifications);
});
