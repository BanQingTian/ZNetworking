using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NRKernal.ObserverView.NetWork
{
    public static class NetworkUtils
    {
        // Get local ipv4, return null if faild.
        public static string GetLocalIPv4()
        {
            string hostName = Dns.GetHostName(); //得到主机名
            IPHostEntry iPEntry = Dns.GetHostEntry(hostName);
            for (int i = 0; i < iPEntry.AddressList.Length; i++)
            {
                //从IP地址列表中筛选出IPv4类型的IP地址
                if (iPEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    return iPEntry.AddressList[i].ToString();
            }
            return null;
        }

        public static string Byte2String(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] String2Byte(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }

    public interface ISerializer
    {
        byte[] Serialize(object obj);

        T Deserialize<T>(byte[] data) where T : class;
    }

    public class JsonSerializer : ISerializer
    {
        public T Deserialize<T>(byte[] data) where T : class
        {
            return LitJson.JsonMapper.ToObject<T>(Encoding.UTF8.GetString(data));
        }

        public byte[] Serialize(object obj)
        {
            return Encoding.UTF8.GetBytes(LitJson.JsonMapper.ToJson(obj));
        }
    }

    public class BinarySerializer : ISerializer
    {
        // obj -> bytes,  return null if obj not mark as [Serializable].
        public byte[] Serialize(object obj)
        {
            //物体不为空且可被序列化
            if (obj == null || !obj.GetType().IsSerializable)
                return null;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                byte[] data = stream.ToArray();
                return data;
            }
        }

        // bytes -> obj, return null if obj not mark as [Serializable].
        public T Deserialize<T>(byte[] data) where T : class
        {
            //数据不为空且T是可序列化的类型
            if (data == null || !typeof(T).IsSerializable)
                return null;
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream(data))
            {
                object obj = formatter.Deserialize(stream);
                return obj as T;
            }
        }
    }

    public static class SerializerFactory
    {
        private static ISerializer _Serializer;

        public static ISerializer Create()
        {
            if (_Serializer == null)
            {
                _Serializer = new JsonSerializer();
            }
            return _Serializer;
        }
    }
}