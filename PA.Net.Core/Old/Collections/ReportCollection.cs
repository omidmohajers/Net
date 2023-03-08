using PA.Net.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PA.Net.Collections
{
    public class ReportCollection : ICollection<NetReport>
    {
        private List<NetReport> reports = new List<NetReport>();
        public event EventHandler NewreportReceived = null;
        public int Count
        {
            get
            {
                return reports.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(NetReport item)
        {
            reports.Add(item);
            if (NewreportReceived != null)
                NewreportReceived(this, EventArgs.Empty);
        }

        public void Add(ReportType repType, string clientName, string msg)
        {
            reports.Add(new NetReport(repType, clientName, msg));
        }

        public void Add(ReportType repType, string clientName, string msg, DateTime date)
        {
            reports.Add(new NetReport(repType, clientName, msg,date));
        }

        public void Add(ReportType repType, string clientName, Exception ex, DateTime date)
        {
            reports.Add(new NetReport(repType, clientName, ex, date));
        }

        public void Add(ReportType repType, string clientName, string msg, Exception ex, DateTime date)
        {
            reports.Add(new NetReport(repType, clientName, msg, ex, date));
        }

        public void Clear()
        {
            reports.Clear();
        }

        public bool Contains(NetReport item)
        {
            return reports.Contains(item);
        }

        public void CopyTo(NetReport[] array, int arrayIndex)
        {
            reports.CopyTo(array, arrayIndex);
        }

        public IEnumerator<NetReport> GetEnumerator()
        {
            return reports.GetEnumerator();
        }

        public bool Remove(NetReport item)
        {
            return reports.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
