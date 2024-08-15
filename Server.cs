using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

public class Server
{
    private TcpListener _listener;
    private const int LISTEN_PORT = 9001;
    private List<TcpClient> _clients = new();
    private bool _running = true;
    private Thread _thread;
    private CancellationTokenSource _cancelSource = new();

    private GameCoordinator _coordinator;


    public Server(GameCoordinator gc)
    {
        _listener = TcpListener.Create(LISTEN_PORT);
        _coordinator = gc;
        _thread = new(() => Listen());
    }

    public void Start()
    {
        _thread.Start();
    }

    public void Stop()
    {
        _running = false;
        _cancelSource.Cancel();
        _cancelSource.Dispose();
        _thread.Join();
        Console.WriteLine("Server: Stopped");
    }

    private async void Listen()
    {
        _listener.Start();
        while (_running)
        {
            TcpClient? client = null;
            try
            {
                client = await _listener.AcceptTcpClientAsync(_cancelSource.Token);
                MessageClient mc = new(client);
                _coordinator.EnqueueClient(mc);
            }
            catch (System.Net.Sockets.SocketException sex)
            {
                Console.WriteLine($"Socket Error: {sex}");
                client?.Close();
                client?.Dispose();
            }
            catch (System.OperationCanceledException)
            {
                client?.Close();
                client?.Dispose();
                Console.WriteLine($"Server.Listen AcceptTcpClientAsync cancelled");
            }
        }
        _listener.Stop();
    }
}
