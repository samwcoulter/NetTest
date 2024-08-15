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


    public Server()
    {
        _listener = TcpListener.Create(LISTEN_PORT);
    }

    public void Start()
    {
        var token = _cancelSource.Token;
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
                    ValidateConnection(client);
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

    private bool ValidateConnection(TcpClient client)
    {
        MessageClient mc = new(client);
        var d = mc.Receive();
        if (d.Item1 == 7)
        {
            Console.WriteLine(Encoding.ASCII.GetString(d.Item2));
        }

        return true;
    }
}
