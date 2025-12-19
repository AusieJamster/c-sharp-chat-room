using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ChatRoom.Models;

namespace ChatRoom.Services;

public class MessageHandler
{
    private readonly ConnectionManager _connectionManager;

    public MessageHandler(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task BroadcastMessageAsync(ChatMessage message, string? excludeConnectionId = null)
    {
        string messageJson = JsonSerializer.Serialize(message);
        byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);
        ArraySegment<byte> arraySegment = new ArraySegment<byte>(messageBytes);

        IEnumerable<KeyValuePair<string, WebSocket>> connections = _connectionManager.GetAllConnections();
        List<Task> sendTasks = new List<Task>();

        foreach (KeyValuePair<string, WebSocket> connection in connections)
        {
            // Skip the excluded connection if specified
            if (excludeConnectionId != null && connection.Key == excludeConnectionId)
                continue;

            if (connection.Value.State == WebSocketState.Open)
            {
                sendTasks.Add(connection.Value.SendAsync(
                    arraySegment,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                ));
            }
        }

        await Task.WhenAll(sendTasks);
    }

    public async Task SendMessageToConnectionAsync(string connectionId, ChatMessage message)
    {
        WebSocket? socket = _connectionManager.GetConnectionById(connectionId);
        if (socket?.State == WebSocketState.Open)
        {
            string messageJson = JsonSerializer.Serialize(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageJson);
            ArraySegment<byte> arraySegment = new ArraySegment<byte>(messageBytes);

            await socket.SendAsync(
                arraySegment,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
    }

    public ChatMessage ParseMessage(string messageText, string senderId)
    {
        try
        {
            // Try to parse as JSON with just content field
            JsonDocument jsonDoc = JsonDocument.Parse(messageText);
            if (jsonDoc.RootElement.TryGetProperty("content", out JsonElement contentElement))
            {
                return new ChatMessage
                {
                    SenderId = senderId,
                    Content = contentElement.GetString() ?? messageText,
                    Timestamp = DateTime.UtcNow,
                    Type = MessageType.UserMessage
                };
            }
        }
        catch
        {
            // If parsing fails, treat the entire message as plain text
        }

        return new ChatMessage
        {
            SenderId = senderId,
            Content = messageText,
            Timestamp = DateTime.UtcNow,
            Type = MessageType.UserMessage
        };
    }

    public ChatMessage CreateSystemMessage(string content, MessageType type = MessageType.SystemMessage)
    {
        return new ChatMessage
        {
            SenderId = "System",
            Content = content,
            Timestamp = DateTime.UtcNow,
            Type = type
        };
    }
}
