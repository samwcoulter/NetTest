using System;
using System.Net;
using System.Net.Sockets;

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

            Span<byte> span = buffer;
            span = span.Slice(sizeof(Int32), sizeof(Int16));

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

    private TcpClient _client;
    private NetworkStream _stream;
    private byte[] _rHeaderBuffer = new byte[Header.HEADER_BUFFER_SIZE];
    private byte[] _sHeaderBuffer = new byte[Header.HEADER_BUFFER_SIZE];

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

    public (Int16, byte[]) Receive()
    {
        _stream.Read(_rHeaderBuffer, 0, Header.HEADER_BUFFER_SIZE);
        Header h = Header.Deserialize(_rHeaderBuffer);
        byte[] buffer = new byte[h.length];
        int bytesRead = 0;
        while (bytesRead < h.length)
        {
            bytesRead += _stream.Read(buffer, bytesRead, h.length - bytesRead);
        }
        return (h.type, buffer);
    }

    public void Send(Int16 messageType, byte[] data)
    {
        Header h = new Header { length = data.Length, type = messageType };
        Header.Serialize(h, _sHeaderBuffer);
        _stream.Write(_sHeaderBuffer, 0, _sHeaderBuffer.Length);
        _stream.Write(data, 0, data.Length);
    }


}
