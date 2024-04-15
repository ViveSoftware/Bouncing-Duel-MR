using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SocketUtils
{
    public const int PORT = 9999;
    public const int LISTEN = 1;
    public const int PACKET_SIZE = 8192;

    public enum SocketRole
    {
        NULL, Server, Client
    }

    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

    [Serializable]
    public class AnchorData
    {
        [Serializable]
        public class acVector3
        {
            public float x;
            public float y;
            public float z;
        }

        [Serializable]
        public class acVector4
        {
            public float x;
            public float y;
            public float z;
            public float w;
        }

        public string anchorName;
        public byte[] anchorBytes;
        public acVector3 anchorPos = new acVector3();
        public acVector4 anchorRot = new acVector4();
    }

    public static byte[] ToByteArray<T>(T obj)
    {
        if (obj == null)
            return null;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream())
        {
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }

    public static T FromByteArray<T>(byte[] data)
    {
        if (data == null)
            return default(T);
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(data))
        {
            object obj = bf.Deserialize(ms);
            return (T)obj;
        }
    }
}
