using Microsoft.AspNetCore.Mvc;
using BE.Patterns.Observer;
using BE.Services;

namespace BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // ================================================================
        // OBSERVER PATTERN - Follow/Unfollow Stories
        // ================================================================

        // POST: api/Notifications/Follow
        [HttpPost("Follow")]
        public async Task<ActionResult> FollowStory([FromBody] FollowDto dto)
        {
            var result = await _notificationService.FollowStoryAsync(dto.UserId, dto.StoryId);

            if (result)
            {
                Console.WriteLine($"[Observer Pattern] User {dto.UserId} followed story {dto.StoryId} successfully");
                return Ok(new { message = "Followed story successfully" });
            }
            else
                return BadRequest(new { message = "Already following this story" });
        }

        // POST: api/Notifications/Unfollow
        [HttpPost("Unfollow")]
        public async Task<ActionResult> UnfollowStory([FromBody] FollowDto dto)
        {
            var result = await _notificationService.UnfollowStoryAsync(dto.UserId, dto.StoryId);

            if (result)
            {
                Console.WriteLine($"[Observer Pattern] User {dto.UserId} unfollowed story {dto.StoryId} successfully");
                return Ok(new { message = "Unfollowed story successfully" });
            }
            else
                return NotFound(new { message = "Not following this story" });
        }

        // GET: api/Notifications/IsFollowing/User/5/Story/10
        [HttpGet("IsFollowing/User/{userId}/Story/{storyId}")]
        public async Task<ActionResult<bool>> IsFollowing(int userId, int storyId)
        {
            return await _notificationService.IsFollowingAsync(userId, storyId);
        }

        // ================================================================
        // Get Notifications
        // ================================================================

        // GET: api/Notifications/User/5
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<object>> GetUserNotifications(int userId, [FromQuery] bool unreadOnly = false)
        {
            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);

            var result = notifications.Select(n => new
            {
                n.NotificationId,
                n.Message,
                n.Type,
                n.IsRead,
                n.CreatedAt,
                Story = new
                {
                    n.Story.StoryId,
                    n.Story.Title
                },
                Chapter = n.Chapter != null ? new
                {
                    n.Chapter.ChapterId,
                    n.Chapter.Title,
                    n.Chapter.ChapterNumber
                } : null
            });

            return Ok(result);
        }

        // PUT: api/Notifications/5/MarkRead
        [HttpPut("{notificationId}/MarkRead")]
        public async Task<ActionResult> MarkNotificationAsRead(int notificationId)
        {
            await _notificationService.MarkAsReadAsync(notificationId);
            return Ok(new { message = "Notification marked as read" });
        }

        // PUT: api/Notifications/User/5/MarkAllRead
        [HttpPut("User/{userId}/MarkAllRead")]
        public async Task<ActionResult> MarkAllNotificationsAsRead(int userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);
            return Ok(new { message = "All notifications marked as read" });
        }
    }

    // DTOs
    public class FollowDto
    {
        public int UserId { get; set; }
        public int StoryId { get; set; }
    }
}
