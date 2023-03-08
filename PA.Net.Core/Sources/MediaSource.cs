using System;

namespace PA.Net.Core.Sources
{
    public delegate void OnNewDataReceive(object sender, EventArgs e);
    public class MediaSource
    {
        public int BitRate { get; set; }
        public virtual byte[] CurrentData { get; set; }
        public event OnNewDataReceive NewDataReceived = null;


        public virtual void RaiseNewDataReceived()
        {
            if (NewDataReceived != null)
                NewDataReceived(this, EventArgs.Empty);
        }

        public virtual void GetNext()
        {

        }

        public virtual void Start()
        {

        }

        public virtual void Stop()
        {

        }

        public virtual void Pause()
        {

        }

        public virtual void Resume()
        {

        }

        public virtual void Dispose()
        {
        }
    }
}
