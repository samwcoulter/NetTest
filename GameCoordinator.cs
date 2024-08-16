using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

public interface IGameCoordinator
{
    public void EnqueueClient(MessageClient messageClient);

    public void Start();

    public void Stop();
}

public class GameCoordinator : IGameCoordinator
{
    private BlockingCollection<MessageClient> _newClients = new();
    private Dictionary<int, Game> _games = new();
    private bool _running = true;
    private Thread _thread;
    private CancellationTokenSource _cancelSource = new();

    public GameCoordinator()
    {
        // TEST CODE
        _games.Add(1, new Game() { Id = 1 });
        _thread = new(() => this.Run());
    }

    public void EnqueueClient(MessageClient messageClient)
    {
        _newClients.Add(messageClient);
    }

    public void Start()
    {
        _thread.Start();
    }

    public void Stop()
    {
        _running = false;
        _cancelSource.Cancel();
        _thread.Join();
    }

    private void Run()
    {
        while (_running)
        {
            try
            {
                if (!_cancelSource.IsCancellationRequested
                        && _newClients.TryTake(out MessageClient? mc, System.Threading.Timeout.Infinite, _cancelSource.Token))
                {
                    ValidateClient(mc);
                }
            }
            catch (System.Net.Sockets.SocketException sex)
            {
                Console.WriteLine($"Socket Error: {sex}");
            }
            catch (System.OperationCanceledException)
            {
                Console.WriteLine($"GameCoordinator.Run Cancelled");
            }
        }
    }

    private async void ValidateClient(MessageClient mc)
    {
        try
        {
            var m = await mc.Receive();
            if (m.type == (Int16)MessageTypes.ConnectionRequest)
            {
                ConnectionRequest r = Serializer.Deserialize<ConnectionRequest>(m.data);
                Console.WriteLine($"{r.User} {r.Game}");
            }
            else
            {
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

    }





}
