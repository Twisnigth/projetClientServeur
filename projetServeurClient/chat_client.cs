using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class ChatClient
{
    private readonly string _serverIP;
    private readonly int _port;
    private TcpClient _client;

    public ChatClient(string serverIP, int port)
    {
        _serverIP = serverIP;
        _port = port;
    }

    public async Task StartAsync()
    {
        _client = new TcpClient();
        await _client.ConnectAsync(_serverIP, _port);
        Console.WriteLine("[CLIENT] Connecté au serveur.");

        NetworkStream stream = _client.GetStream();
        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        Console.Write("Identifiant : ");
        string username = Console.ReadLine();

        Console.Write("Mot de passe : ");
        string password = Console.ReadLine();

        await writer.WriteLineAsync(username);
        await writer.WriteLineAsync(password);

        string response = await reader.ReadLineAsync();
        if (response == "AUTH_FAIL")
        {
            Console.WriteLine("Identifiants invalides.");
            _client.Close();
            return;
        }

        Console.WriteLine("Authentification réussie !");
        Console.WriteLine("Tapez 'exit' pour quitter.\n");

        _ = Task.Run(async () =>
        {
            while (true)
            {
                string incoming = await reader.ReadLineAsync();
                if (incoming == null) break;
                Console.Write("\r" + new string(' ', Console.WindowWidth) + "\r"); // Efface la ligne
                Console.WriteLine(incoming);
                Console.Write("Message : ");
            }
        });

        while (true)
        {
            Console.Write("Message : ");
            string message = Console.ReadLine();
            await writer.WriteLineAsync(message);

            if (message.Trim().ToLower() == "exit") break;
        }

        _client.Close();
        Console.WriteLine("[CLIENT] Déconnecté.");
    }
}
