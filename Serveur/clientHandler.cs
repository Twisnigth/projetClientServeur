using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ClientHandler
{
    private readonly TcpClient _client;
    private readonly ChatServer _server;
    private string _clientName = "";
    private NetworkStream _stream;

    public ClientHandler(TcpClient client, ChatServer server)
    {
        _client = client;
        _server = server;
    }

    public async Task HandleAsync()
    {
        _stream = _client.GetStream();
        StreamReader reader = new StreamReader(_stream, Encoding.UTF8);
        StreamWriter writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

        string username = await reader.ReadLineAsync();
        string password = await reader.ReadLineAsync();

        if (!_server.IsValidUser(username, password))
        {
            await writer.WriteLineAsync("AUTH_FAIL");
            _client.Close();
            return;
        }

        await writer.WriteLineAsync("AUTH_OK");
        _clientName = username;
        Console.WriteLine($"[SERVEUR] {_clientName} connecté.");
        _server.AddClient(this);

        foreach (var line in _server.GetHistory())
            await SendMessageAsync(line);

        await _server.BroadcastMessageToOthersAsync("Serveur", $"{_clientName} a rejoint le chat", this);

        try
        {
            while (true)
            {
                string message = await reader.ReadLineAsync();
                if (message == null || message.ToLower() == "exit") break;

                Console.WriteLine($"[{_clientName}] {message}");
                await _server.BroadcastMessageAsync(_clientName, message);
            }
        }
        catch { }

        Console.WriteLine($"[SERVEUR] {_clientName} a quitté.");
        await _server.BroadcastMessageToOthersAsync("Serveur", $"{_clientName} a quitté le chat", this);
        _server.RemoveClient(this);
        _client.Close();
    }

    public async Task SendMessageAsync(string message)
    {
        try
        {
            if (_stream != null && _client.Connected)
            {
                StreamWriter writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };
                await writer.WriteLineAsync(message);
            }
        }
        catch { }
    }
}
