using System.Net;

namespace PA.Net.Core
{
    public class FileDownloadState
    {
        public long FileID { get; set; }
        public IPAddress ClientIP { get; set; }
        public int ClientPort { get; set; }
        public long FileSize { get; set; }
        public long Start { get; set; }
        public long End { get; set; }
    }
}
