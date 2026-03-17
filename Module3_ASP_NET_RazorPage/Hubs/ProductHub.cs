
using Microsoft.AspNetCore.SignalR;

namespace Module3_ASP_NET_RazorPage.Hubs;

public static class NotificationStore
{
    private static List<ProductNotification> _notifications = new();
    private static object _lock = new object();
    
    public static List<ProductNotification> GetNotifications()
    {
        lock (_lock)
        {
            return _notifications.OrderByDescending(x => x.Timestamp).ToList();
        }
    }
    
    public static void AddNotification(ProductNotification notification)
    {
        lock (_lock)
        {
            _notifications.Add(notification);
            if (_notifications.Count > 100)
                _notifications.RemoveAt(0);
        }
    }
}

public class ProductNotification
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string NotificationType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class ProductHub : Hub
{
    public async Task SendProductNotify(int productId, string productName, string notificationType)
    {
        var notification = new ProductNotification
        {
            ProductId = productId,
            ProductName = productName,
            NotificationType = notificationType,
            Timestamp = DateTime.Now
        };
        
        NotificationStore.AddNotification(notification);
        await Clients.All.SendAsync("ReceiveNotification", notification);
    }
}

