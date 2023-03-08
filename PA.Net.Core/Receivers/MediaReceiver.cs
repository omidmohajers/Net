using PA.Codec;
using PA.Net.Clients;
using System;

namespace PA.Net.Core.Receivers
{
    public class MediaReceiver
    {
        public event ExceptionReportHandler ExceptionReported;
        private long userID;
        private long roomID;

        public MediaReceiver(INetClient client,long usrID, long room)
        {
            Client = client;
            userID = usrID;
            roomID = room;
        }

        public virtual INetClient Client
        {
            get;
            set;
        }

        public virtual ICodec Codec
        {
            get;
            set;
        }

        public long UserID
        {
            get
            {
                return userID;
            }
        }

        public long RoomID
        {
            get
            {
                return roomID;
            }
        }

        public virtual bool Start()
        {
            return true;
        }

        public virtual void ListenToSideEvents()
        {
        }

        public void RaiseExceptionReported(Exception ex)
        {
            if (ExceptionReported != null)
                ExceptionReported(this, ex);
        }
    }
}
