using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace PA.Net.Core
{
    [Serializable]
    public class Package
    {
        public Package(CommandType type, IPAddress targetMachine, IPAddress senderIP, string message, long? roomID)
        {
            CommandType = type;
            Target = targetMachine;
            Data = Encoding.UTF8.GetBytes(message ?? string.Empty);
            RoomID = roomID ?? 0;
            SenderIP = senderIP;
            Time = DateTime.Now;
        }
        public long UserID { get; set; }
        public long RoomID { get; set; }
        public IPAddress SenderIP { get; set; }
        public CommandType CommandType { get; set; }
        public IPAddress Target { get; set; }
        public object Data { get; set; }

        public DateTime Time
        {
            get; set;
        }
        public int SenderPort { get; set; }

        public static byte[] ToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static Package FromByteArray(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                Package obj = binForm.Deserialize(memStream) as Package;
                return obj;
            }
        }
    }
}
