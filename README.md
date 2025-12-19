# C# WebSocket Chat Room

A real-time chat messaging service built with ASP.NET Core WebSockets that allows multiple clients to connect and exchange messages.

## Features

- 🔌 **WebSocket Support**: Real-time bidirectional communication
- 👥 **Multi-Client**: Support for multiple concurrent connections
- 📨 **Message Broadcasting**: Messages sent to all connected clients
- 🔔 **Connection Notifications**: Join/leave notifications for all users
- 🆔 **Unique Client IDs**: Each connection gets a unique identifier

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## Getting Started

### 1. Clone or navigate to the repository

```bash
cd G:\Users\Ausie\Documents\GitHub\c-sharp-chat-room
```

### 2. Build the project

```bash
dotnet build
```

### 3. Run the server

```bash
dotnet run
```

The server will start and listen on `http://localhost:5000`. You should see output indicating the server is running.

### 4. Test with Postman

#### Connect First Client:

1. Open Postman
2. Click **New** → **WebSocket Request**
3. Enter the URL: `ws://localhost:5000/ws`
4. Click **Connect**
5. You should receive a welcome message with your unique client ID

#### Connect Second Client:

1. Open a new Postman tab or window
2. Create another WebSocket request to `ws://localhost:5000/ws`
3. Click **Connect**
4. Both clients should receive a notification that a new user has joined

#### Send Messages:

Send messages in JSON format:

```json
{
  "content": "Hello from Client 1!"
}
```

Or send plain text (it will be automatically wrapped):

```
Hello everyone!
```

All connected clients will receive the message with sender information.

## Message Format

### Sending Messages (Client → Server)

```json
{
  "content": "Your message text"
}
```

### Receiving Messages (Server → Client)

```json
{
  "senderId": "abc-123-def",
  "content": "Message text",
  "timestamp": "2025-12-19T03:16:35.123Z",
  "type": 0
}
```

**Message Types:**

- `0` - UserMessage
- `1` - SystemMessage
- `2` - ConnectionNotification
- `3` - DisconnectionNotification

## Project Structure

```
c-sharp-chat-room/
├── Models/
│   └── ChatMessage.cs          # Message data model
├── Services/
│   ├── ConnectionManager.cs    # Manages WebSocket connections
│   └── MessageHandler.cs       # Handles message routing
├── Program.cs                  # Application entry point
├── appsettings.json           # Configuration
└── c-sharp-chat-room.csproj   # Project file
```

## How It Works

1. **Connection**: Client connects via WebSocket to `/ws` endpoint
2. **Registration**: Server assigns unique ID and adds to connection pool
3. **Welcome**: Client receives welcome message with their ID
4. **Broadcast**: New connection is announced to all other clients
5. **Messaging**: Any message sent by a client is broadcast to all connected clients
6. **Disconnection**: When a client disconnects, all others are notified

## Console Output

The server logs connection events and messages to the console:

```
[13:16:35] Client connected: abc-123-def
[13:16:40] Message from abc-123-def: {"content":"Hello!"}
[13:17:00] Client disconnected: abc-123-def
```

## Troubleshooting

**Port already in use?**

- Change the port in `appsettings.json` under `Kestrel.Endpoints.Http.Url`

**Connection refused?**

- Ensure the server is running (`dotnet run`)
- Check firewall settings

**Messages not appearing?**

- Verify JSON format when sending messages
- Check server console for error messages

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
