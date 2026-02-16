using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using VendorManagementSystem.Server.Hubs;
using VendorManagementSystem.Server.Services;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Notifications;

namespace VendorManagementSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationsController(INotificationService notificationService, IHubContext<NotificationHub> hubContext)
    {
        _notificationService = notificationService;
        _hubContext = hubContext;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<NotificationDto>>>> GetNotifications([FromQuery] bool unreadOnly = false)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
        return Ok(ApiResponse<IEnumerable<NotificationDto>>.Ok(notifications));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(ApiResponse<int>.Ok(count));
    }

    [HttpPost("{id:int}/read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await _notificationService.MarkAsReadAsync(id, userId);

        var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
        await _hubContext.Clients.Group(userId)
            .SendAsync(NotificationHub.Methods.UnreadCountUpdated, unreadCount);

        return Ok();
    }

    [HttpPost("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await _notificationService.MarkAllAsReadAsync(userId);

        await _hubContext.Clients.Group(userId)
            .SendAsync(NotificationHub.Methods.UnreadCountUpdated, 0);

        return Ok();
    }
}
