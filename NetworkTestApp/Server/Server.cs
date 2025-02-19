using System.Net;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Threading;
using NetworkTestApp.Shared;

namespace NetworkTestApp.Server;

public class Server
{
    private IPEndPoint localEndPoint;
    private TcpListener tcpListener;

    public bool IsActive = false;
    
    private List<TcpClient> clients = new List<TcpClient>();
    
    public void Start()
    {
        Console.WriteLine("Starting server...");

        var localIpAddress = IPAddress.Parse("127.0.0.1");
        localEndPoint = new IPEndPoint(localIpAddress, 8080);
        tcpListener = new TcpListener(localEndPoint);

        Console.WriteLine("Listening for connections...");
        Thread thread = new Thread(new ThreadStart(ListenForTcpClients));
        thread.Start();
        IsActive = true;
    }

    private void ListenForTcpClients()
    {
        tcpListener.Start();

        while (IsActive)
        {
            TcpClient client = tcpListener.AcceptTcpClient();
            Thread clientThread = new Thread(new ParameterizedThreadStart(WorkWithClient));
            Console.WriteLine("New client connected");

            clients.Add(client);
            clientThread.Start(client);
            Thread.Sleep(15);
        }
    }

    private void DisconnectClient(TcpClient client)
    {
        if (client == null) return;
        
        Console.WriteLine("Disconnected client: " + client.ToString());
        
        client.Close();
        
        clients.Remove(client);
    }

    private void WorkWithClient(object client)
    {
        TcpClient tcpClient = client as TcpClient;

        if (tcpClient == null)
        {
            Console.WriteLine("Client is null, stopping processing for this client!");
            DisconnectClient(tcpClient);
            return;
        }
        
        NetworkStream clientStream = tcpClient.GetStream();
        int bytesRead;
        
        while (IsActive)
        {
            bytesRead = 0;

            var bytesBuffer = new byte[Packet.Length];
            try
            {
                bytesRead = clientStream.Read(bytesBuffer, 0, bytesBuffer.Length);
            }
            catch
            {
                Console.WriteLine("A socket error has occurred with client: " + tcpClient.ToString());
                break;
            }

            if (bytesRead == 0) break;
            
            // TODO: Process incoming data
            var packet = new Packet(bytesBuffer);
            
            Console.WriteLine(packet);

            Thread broadcastThread = new Thread(new ParameterizedThreadStart(BroadcastMessage));
            broadcastThread.Start(packet);
        }
        
        DisconnectClient(tcpClient);
    }

    private void BroadcastMessage(object? broadcastPacket)
    {
        Packet packet = broadcastPacket as Packet;
        
        if (broadcastPacket == null) return;
        
        foreach (var client in clients)
        {
            var networkStream = client.GetStream();
            networkStream.Write(packet.ToByteArray());
        }
    }
}