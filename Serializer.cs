using System.Text;
using System.Text.Json;

public class Serializer
{
    public static byte[] Serialize(object o)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(o));
    }
    public static T Deserialize<T>(byte[] d)
    {
        T? t = JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(d));
        return t ?? throw new System.Exception("Unable to deserialize data");
    }
}
