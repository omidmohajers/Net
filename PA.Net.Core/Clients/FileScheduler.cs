using System.Collections.Generic;

namespace PA.Net.Core
{
    public class FileScheduler
    {
        public List<FileTransferController> Files = new List<FileTransferController>();

        public FileTransferController Find(FileTransferController ftp)
        {
            FileTransferController result = Files.Find(item => (item.FileID > 0 && item.FileID == ftp.FileID) ||
                                       (item.FileID == 0 && item.FileName == ftp.FileName && item.FileSize == ftp.FileSize && ftp.Start == 0 && ftp.End == 0));
            if (ftp.FileID > 0 && result != null)
                result.FileID = ftp.FileID;
            return result;
        }

        public FileTransferController Find(long fileID)
        {
            FileTransferController result = Files.Find(item => (item.FileID > 0 && item.FileID == fileID));
            return result;
        }
    }
}
