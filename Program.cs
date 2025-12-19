using System.Net.WebSockets;
using System.Text;
using ChatRoom.Models;
using ChatRoom.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<ConnectionManager>();
builder.Services.AddSingleton<MessageHandler>();

// Add CORS for any origin (useful for testing)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

WebApplication app = builder.Build();

app.UseCors();

// Enable WebSocket support
app.UseWebSockets();

// WebSocket endpoint
app.Map("/ws", async (HttpContext context, ConnectionManager connectionManager, MessageHandler messageHandler) =>
{
    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsync("Expected a WebSocket request");
        return;
    }

    using WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
    string connectionId = connectionManager.AddConnection(webSocket);
    
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client connected: {connectionId}");

    // Send welcome message to the new connection
    ChatMessage welcomeMessage = messageHandler.CreateSystemMessage(
        $"Welcome! You are connected as {connectionId}. Total connections: {connectionManager.GetConnectionCount()}",
        MessageType.SystemMessage
    );
    await messageHandler.SendMessageToConnectionAsync(connectionId, welcomeMessage);

    // Notify all other clients about the new connection
    ChatMessage joinNotification = messageHandler.CreateSystemMessage(
        $"User {connectionId} has joined the chat",
        MessageType.ConnectionNotification
    );
    await messageHandler.BroadcastMessageAsync(joinNotification, excludeConnectionId: connectionId);

    try
    {
        byte[] buffer = new byte[1024 * 4];
        WebSocketReceiveResult result;

        while (webSocket.State == WebSocketState.Open)
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            if (result.MessageType == WebSocketMessageType.Text)
            {
                string messageText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Message from {connectionId}: {messageText}");

                // Parse and broadcast the message
                ChatMessage chatMessage = messageHandler.ParseMessage(messageText, connectionId);
                await messageHandler.BroadcastMessageAsync(chatMessage);
            }
        }
    }
    catch (WebSocketException ex)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] WebSocket error for {connectionId}: {ex.Message}");
    }
    finally
    {
        // Remove the connection and notify others
        connectionManager.RemoveConnection(connectionId);
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client disconnected: {connectionId}");

        ChatMessage leaveNotification = messageHandler.CreateSystemMessage(
            $"User {connectionId} has left the chat",
            MessageType.DisconnectionNotification
        );
        await messageHandler.BroadcastMessageAsync(leaveNotification);

        if (webSocket.State != WebSocketState.Aborted && webSocket.State != WebSocketState.Closed)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
        }
    }
});

// Root endpoint with instructions
app.MapGet("/", () => Results.Text(@"
WebSocket Chat Server is running!

To connect using Postman:
1. Create a new WebSocket request
2. Connect to: ws://localhost:5000/ws
3. Send messages in JSON format: {""content"": ""Your message here""}

You can connect multiple Postman instances to test multi-client chat.
", "text/plain"));

app.Run();
