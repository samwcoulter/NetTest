using System.Collections.Concurrent;
class Game
{
    public int Id { get; set; }
    private ConcurrentQueue<MessageClient.Message> _messages = new();

}
