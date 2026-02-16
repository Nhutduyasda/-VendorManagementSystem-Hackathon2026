using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using VendorManagementSystem.Shared.DTOs.Notifications;

namespace VendorManagementSystem.Server.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public static class Methods
    {
        public const string ReceiveNotification = "ReceiveNotification";
        public const string NotificationRead = "NotificationRead";
        public const string UnreadCountUpdated = "UnreadCountUpdated";
    }
}
