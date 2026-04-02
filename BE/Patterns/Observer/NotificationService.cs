using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Patterns.Observer
{
    // ================================================================
    // OBSERVER PATTERN - Subject Interface
    // ================================================================
    public interface IStorySubject
    {
        Task NotifyFollowersAsync(int storyId, string message, int? chapterId = null);
    }

    // ================================================================
    // OBSERVER PATTERN - Notification Service (Subject Implementation)
    // ================================================================
    public class NotificationService : IStorySubject
    {
        private readonly StoryReaderDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(
            StoryReaderDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Notify all observers (followers) about new chapter
        public async Task NotifyFollowersAsync(int storyId, string message, int? chapterId = null)
        {
            // Get all followers (observers) of this story
            var followers = await _context.StoryFollowers
                .Where(sf => sf.StoryId == storyId)
                .Select(sf => sf.UserId)
                .ToListAsync();

            // Create notifications for all followers
            var notifications = followers.Select(userId => new Notification
            {
                UserId = userId,
                StoryId = storyId,
                ChapterId = chapterId,
                Message = message,
                Type = "NewChapter",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            if (notifications.Any())
            {
                await _context.Notifications.AddRangeAsync(notifications);
                await _context.SaveChangesAsync();

                // SIGNALR: Send real-time notifications
                var notificationData = new
                {
                    StoryId = storyId,
                    ChapterId = chapterId,
                    Message = message,
                    Type = "NewChapter",
                    CreatedAt = DateTime.UtcNow
                };

                // Send to story group
                await _hubContext.Clients.Group($"story_{storyId}")
                    .SendAsync("ReceiveNotification", notificationData);

                // Send to each user's group
                foreach (var userId in followers)
                {
                    await _hubContext.Clients.Group($"user_{userId}")
                        .SendAsync("ReceiveNotification", notificationData);
                }
            }
        }

        // Add observer (follow story)
        public async Task<bool> FollowStoryAsync(int userId, int storyId)
        {
            // Check if already following
            var exists = await _context.StoryFollowers
                .AnyAsync(sf => sf.UserId == userId && sf.StoryId == storyId);

            if (exists)
                return false;

            var follower = new StoryFollower
            {
                UserId = userId,
                StoryId = storyId,
                CreatedAt = DateTime.UtcNow
            };

            _context.StoryFollowers.Add(follower);
            await _context.SaveChangesAsync();
            return true;
        }

        // Remove observer (unfollow story)
        public async Task<bool> UnfollowStoryAsync(int userId, int storyId)
        {
            var follower = await _context.StoryFollowers
                .FirstOrDefaultAsync(sf => sf.UserId == userId && sf.StoryId == storyId);

            if (follower == null)
                return false;

            _context.StoryFollowers.Remove(follower);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsFollowingAsync(int userId, int storyId)
        {
            return await _context.StoryFollowers
                .AnyAsync(sf => sf.UserId == userId && sf.StoryId == storyId);
        }

        // Get user notifications
        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false)
        {
            var query = _context.Notifications
                .Include(n => n.Story)
                .Include(n => n.Chapter)
                .Where(n => n.UserId == userId);

            if (unreadOnly)
                query = query.Where(n => !n.IsRead);

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        // Mark notification as read
        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        // Mark all user notifications as read
        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
