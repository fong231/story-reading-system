using Microsoft.AspNetCore.SignalR;

namespace BE.Models
{
    // ================================================================
    // OBSERVER PATTERN + SignalR
    // Real-time notification hub
    // ================================================================
    public class NotificationHub : Hub
    {
        // Client gọi để join vào nhóm nhận thông báo cho story
        public async Task JoinStoryGroup(int storyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"story_{storyId}");
        }

        // Client gọi để rời khỏi nhóm
        public async Task LeaveStoryGroup(int storyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"story_{storyId}");
        }

        // Client gọi để join vào nhóm user (nhận tất cả notifications của user)
        public async Task JoinUserGroup(int userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        // Server gọi để gửi notification cho tất cả followers của story
        public async Task SendStoryNotification(int storyId, object notification)
        {
            await Clients.Group($"story_{storyId}").SendAsync("ReceiveNotification", notification);
        }

        // Server gọi để gửi notification cho user cụ thể
        public async Task SendUserNotification(int userId, object notification)
        {
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
        }
    }
}