using System.Collections.Generic;

namespace ItemShopHub.Shared.Models;

public class NotificationDispatchResponse
{
    public int CreatedCount { get; set; }
    public IReadOnlyList<Notification> Notifications { get; set; } = new List<Notification>();
}
