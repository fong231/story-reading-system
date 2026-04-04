using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using BE.Models;

namespace BE.Patterns.Observer
{
    // ================================================================
    // OBSERVER PATTERN - Story Observer
    // Notifies all observers (followers) about new chapter
    // ================================================================
    public interface IStoryObserver
    {
        Task NotifyFollowersAsync(int storyId, string message, int? chapterId = null);
    }
    
    public class StoryObserver : IStoryObserver
    {
        private readonly StoryReaderDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public StoryObserver(
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
    }
}
