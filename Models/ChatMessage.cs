namespace ChatRoom.Models;

public class ChatMessage
{
    public string SenderId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public MessageType Type { get; set; }
}

public enum MessageType
{
    UserMessage,
    SystemMessage,
    ConnectionNotification,
    DisconnectionNotification
}
