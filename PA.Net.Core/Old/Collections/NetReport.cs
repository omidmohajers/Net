using PA.Net.Core;
using System;

namespace PA.Net.Collections
{
    public class NetReport
    {
        public ReportType Type { get; }
        public string ClientName { get; }
        public string Message { get; }
        public Exception Exception { get; }
        public DateTime Date { get; }
        public NetReport(ReportType repType, string clientName, string msg)
        {
            Type = repType;
            ClientName = clientName;
            Message = msg;
            Date = DateTime.Now;
            Exception = null;
        }

        public NetReport(ReportType repType, string clientName, string msg, DateTime date)
        {
            Type = repType;
            ClientName = clientName;
            Message = msg;
            Date = date;
            Exception = null;
        }

        public NetReport(ReportType repType, string clientName, Exception ex, DateTime date)
        {
            Type = repType;
            ClientName = clientName;
            Message = ex == null? string.Empty : ex.Message;
            Date = date;
            Exception = ex;
        }

        public NetReport(ReportType repType, string clientName,string msg, Exception ex, DateTime date)
        {
            Type = repType;
            ClientName = clientName;
            Message = msg;
            Date = date;
            Exception = ex;
        }

    }
}
