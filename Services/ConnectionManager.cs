using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace ChatRoom.Services;

public class ConnectionManager
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

    public string AddConnection(WebSocket socket)
    {
        string connectionId = Guid.NewGuid().ToString();
        _connections.TryAdd(connectionId, socket);
        return connectionId;
    }

    public void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public WebSocket? GetConnectionById(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var socket);
        return socket;
    }

    public IEnumerable<KeyValuePair<string, WebSocket>> GetAllConnections()
    {
        return _connections.ToList();
    }

    public int GetConnectionCount()
    {
        return _connections.Count;
    }
}
