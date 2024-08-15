using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public interface IGameCoordinator
{
    public void EnqueueClient(MessageClient messageClient);

    public void Start();

    public Task Stop();
}

public class GameCoordinator : IGameCoordinator
{
    private BlockingCollection<MessageClient> _newClients = new();
    private Dictionary<int, Game> _games = new();
    private bool _running = true;
    private CancellationTokenSource _cancelSource = new();

    public GameCoordinator()
    {
        // TEST CODE
        _games.Add(1, new Game() { Id = 1 });
    }

    public void EnqueueClient(MessageClient messageClient)
    {
        _newClients.Add(messageClient);
    }

    public void Start()
    {
        Task.Run(async () => await Run());
    }

    public async Task Stop()
    {
        _running = false;
        if (!_cancelSource.IsCancellationRequested)
        {
            await _cancelSource.CancelAsync();
        }
    }

    private async Task Run()
    {
        while (_running)
        {
            if (!_cancelSource.IsCancellationRequested
                    && _newClients.TryTake(out MessageClient? mc, System.Threading.Timeout.Infinite, _cancelSource.Token))
            {
                var t = Task.Run(async () => await ValidateClient(mc));
            }
        }
    }

    private async Task ValidateClient(MessageClient mc)
    {
        var m = await mc.Receive();
        if (m.type == 7)
        {
            Console.WriteLine(Encoding.ASCII.GetString(m.data));
        }

    }





}
