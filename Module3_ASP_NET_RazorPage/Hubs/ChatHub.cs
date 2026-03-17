using Microsoft.AspNetCore.SignalR;

namespace Module3_ASP_NET_RazorPage.Hubs;

public class ChatMessage
{
    public string UserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public static class ChatMessageStore
{
    private static List<ChatMessage> _messages = new();
    private static object _lock = new object();

    public static List<ChatMessage> GetMessages()
    {
        lock (_lock)
        {
            return _messages.OrderBy(x => x.Timestamp).ToList();
        }
    }

    public static void AddMessage(ChatMessage message)
    {
        lock (_lock)
        {
            _messages.Add(message);
            if (_messages.Count > 500)
                _messages.RemoveAt(0);
        }
    }
}

public class ChatHub : Hub
{
    public async Task SendMessage(string userName, string message)
    {
        var chatMessage = new ChatMessage
        {
            UserName = userName,
            Message = message,
            Timestamp = DateTime.Now
        };

        ChatMessageStore.AddMessage(chatMessage);
        await Clients.All.SendAsync("ReceiveMessage", chatMessage);
    }
}

