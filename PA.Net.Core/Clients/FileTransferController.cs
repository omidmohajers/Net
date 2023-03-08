using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PA.Net.Core
{
    [Serializable]
    public class FileTransferController
    {
        public long FileID { get; set; }
        public long FileSize { get; set; }
        public TransferSide TransferSide { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string LocalPath { get; set; }

        public static byte[] ToByteArray(Object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }
        public static FileTransferController FromByteArray(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                FileTransferController obj = binForm.Deserialize(memStream) as FileTransferController;
                return obj;
            }
        }
    }
}
