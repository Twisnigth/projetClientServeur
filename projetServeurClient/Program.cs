using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        ChatClient client = new ChatClient("127.0.0.1", 5050);
        await client.StartAsync();
    }
}