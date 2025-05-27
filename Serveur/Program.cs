using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        ChatServer server = new ChatServer(5050);
        await server.StartAsync();
    }
}
