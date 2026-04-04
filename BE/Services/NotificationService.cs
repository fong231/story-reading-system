using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Services
{
    public interface INotificationService
    {
        Task<bool> FollowStoryAsync(int userId, int storyId);
        Task<bool> UnfollowStoryAsync(int userId, int storyId);
        Task<bool> IsFollowingAsync(int userId, int storyId);
        Task<List<Notification>> GetUserNotificationsAsync(int userId, bool unreadOnly = false);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(int userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly StoryReaderDbContext _context;

        public NotificationService(StoryReaderDbContext context)
        {
            _context = context;
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
