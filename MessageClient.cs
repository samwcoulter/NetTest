using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class MessageClient
{
    public struct Header
    {
        public const int HEADER_BUFFER_SIZE = sizeof(Int32) + sizeof(Int16);
        public Int32 length;
        public Int16 type;

        static public void Serialize(Header h, byte[] buffer)
        {
            Int32 length = IPAddress.HostToNetworkOrder(h.length);
            Int16 type = IPAddress.HostToNetworkOrder(h.type);
            bool wroteLength = BitConverter.TryWriteBytes(buffer, length);

            Span<byte> span = new(buffer, sizeof(Int32), sizeof(Int16));
            bool wroteType = BitConverter.TryWriteBytes(span, type);

            if (!(wroteLength && wroteType))
            {
                throw new Exception($"Unable to serialize {h}");
            }
        }

        static public Header Deserialize(byte[] buffer)
        {
            Int32 length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
            Int16 type = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, sizeof(Int32)));
            return new Header { length = length, type = type };
        }
    }

    public struct Message
    {
        public Int16 type;
        public byte[] data;
    }

    private TcpClient _client;
    private NetworkStream _stream;

    public MessageClient(IPAddress a, int port)
    {
        TcpClient client = new();
        client.Connect(a, port);
        _client = client;
        _stream = client.GetStream();
    }

    public MessageClient(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
    }

    ~MessageClient()
    {
        _stream.Close();
        _client.Dispose();
    }

    public async Task<Message> Receive()
    {
        byte[] headerBuffer = new byte[Header.HEADER_BUFFER_SIZE];
        int headerBytesRead = 0;
        while (headerBytesRead < Header.HEADER_BUFFER_SIZE)
        {
            headerBytesRead += await _stream.ReadAsync(headerBuffer, headerBytesRead, Header.HEADER_BUFFER_SIZE - headerBytesRead);
        }
        Header h = Header.Deserialize(headerBuffer);
        Message m = new();
        m.data = new byte[h.length];
        m.type = h.type;
        int bytesRead = 0;
        while (bytesRead < h.length)
        {
            bytesRead += await _stream.ReadAsync(m.data, bytesRead, h.length - bytesRead);
        }
        return m;
    }

    public async Task Send(Message m)
    {
        await Send(m.type, m.data);
    }

    public async Task Send(Int16 messageType, byte[] data)
    {
        byte[] headerBuffer = new byte[Header.HEADER_BUFFER_SIZE];
        Header h = new Header { length = data.Length, type = messageType };
        Header.Serialize(h, headerBuffer);
        await _stream.WriteAsync(headerBuffer, 0, headerBuffer.Length);
        await _stream.WriteAsync(data, 0, data.Length);
    }


}
