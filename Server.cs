using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Server
{
    private TcpListener _listener;
    private const int LISTEN_PORT = 9001;
    private List<TcpClient> _clients = new();
    private bool _running = true;
    private CancellationTokenSource _cancelSource = new();

    private GameCoordinator _coordinator;


    public Server(GameCoordinator gc)
    {
        _listener = TcpListener.Create(LISTEN_PORT);
        _coordinator = gc;
    }

    public void Start()
    {
        Task.Run(async () => await Listen());
    }

    public async Task Stop()
    {
        _running = false;
        await _cancelSource.CancelAsync();
        Console.WriteLine("Server: Stopped");
        _cancelSource.Dispose();
    }

    private async Task Listen()
    {
        try
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
            }
            _listener.Stop();
        }
        catch (System.OperationCanceledException)
        {
            Console.WriteLine($"Server.Listen AcceptTcpClientAsync cancelled");
        }
    }
}
