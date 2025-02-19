namespace NetworkTestApp;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0) return;
        
        if (args[0].ToLower().EndsWith("server"))
        {
            var server = new Server.Server();
            server.Start();
        }
        else
        {
            var client = new Client.Client();
            client.Start();
        }
    }
}