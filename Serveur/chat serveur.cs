using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class ChatServer
{
    private readonly int _port;
    private TcpListener _listener;
    private readonly List<ClientHandler> _clients = new();
    private readonly List<string> _messageHistory = new();
    private readonly Dictionary<string, string> _users = new()
    {
        { "enzo", "1234" },
        { "bob", "azerty" },
        { "admin", "adminpass" }
    };

    public ChatServer(int port)
    {
        _port = port;
    }

    public bool IsValidUser(string username, string password)
    {
        return _users.ContainsKey(username) && _users[username] == password;
    }

    public async Task StartAsync()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        Console.WriteLine($"[SERVEUR] En attente de connexions sur le port {_port}");

        while (true)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();
            var handler = new ClientHandler(client, this);
            _ = handler.HandleAsync();
        }
    }

    public async Task BroadcastMessageAsync(string sender, string message)
    {
        string fullMessage = $"[{sender}] {message}";
        _messageHistory.Add(fullMessage);

        foreach (var client in _clients)
        {
            await client.SendMessageAsync(fullMessage);
        }
    }

    public async Task BroadcastMessageToOthersAsync(string sender, string message, ClientHandler exceptClient)
    {
        string fullMessage = $"[{sender}] {message}";
        _messageHistory.Add(fullMessage);

        foreach (var client in _clients)
        {
            if (client != exceptClient)
            {
                await client.SendMessageAsync(fullMessage);
            }
        }
    }

    public List<string> GetHistory() => _messageHistory;
    public void AddClient(ClientHandler c) => _clients.Add(c);
    public void RemoveClient(ClientHandler handler) => _clients.Remove(handler);
}
