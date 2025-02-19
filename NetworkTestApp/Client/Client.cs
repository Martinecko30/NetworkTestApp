using System.Net;
using System.Net.Sockets;
using NetworkTestApp.Shared;

namespace NetworkTestApp.Client;

public class Client
{
    private TcpClient myClient;
    private EndPoint serverEndPoint;

    private bool IsActive = false;

    private string username = "Client";
    
    public void Start()
    {
        Console.Write("Enter Username: ");
        var userInput = Console.ReadLine();
        if (userInput != null)
            username = userInput.Trim();

        IPAddress ipAddress;
        try
        {
            ipAddress = IPAddress.Parse("127.0.0.1");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        if (ipAddress == null)
        {
            Console.WriteLine("Please input a valid IP address.");
            return;
        }
        
        IPEndPoint endPoint = new IPEndPoint(ipAddress, 8080);

        Random random = new Random();
        var randomPort = random.Next(10000, 11000);
        
        myClient = new TcpClient(new IPEndPoint(IPAddress.Any, randomPort));
        
        myClient.Connect(endPoint);

        Thread listenThread = new Thread(new ThreadStart(ListenToServer));
        Thread writeThread = new Thread(new ThreadStart(WriteToServer));
        
        IsActive = true;
        listenThread.Start();
        writeThread.Start();
    }

    private void ListenToServer()
    {
        int bytesRead;
        while (IsActive)
        {
            bytesRead = 0;
            
            byte[] bytesBuffer = new byte[Packet.Length];
            try
            {
                NetworkStream networkStream = myClient.GetStream();

                bytesRead = networkStream.Read(bytesBuffer, 0, bytesBuffer.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("An error occured.");
                return;
            }

            if (bytesRead == 0) break;
            
            // TODO: Process bytes from server
            Packet packet = new Packet(bytesBuffer);
            Console.WriteLine(packet.ToString());
        }
    }

    private void WriteToServer()
    {
        while (IsActive)
        {
            var userInput = Console.ReadLine();
            
            if (userInput == null)
                return;
            
            Packet packet = new Packet(username, MessageType.Broadcast, userInput);

            try
            {
                NetworkStream networkStream = myClient.GetStream();
                
                networkStream.Write(packet.ToByteArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("An error occured.");
                return;
            }
        }
    }
}