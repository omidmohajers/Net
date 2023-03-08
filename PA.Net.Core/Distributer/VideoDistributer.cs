using System;
using PA.Codec;
using PA.Net.Clients;
using PA.Net.Core;
using PA.Net.Core.Sources;

namespace PA.Distributer
{
    public class VideoDistributer : RealtimeDistributer
    {
        public event DataReceiveHandler VideoFrameFromSourceReceived = null;
        public VideoDistributer(INetClient client, long userID, long roomID) : base(client, userID, roomID)
        {

        }
        public VideoDistributer(INetClient client, VideoCodecTypes videoType, long userID, long roomID) : this(client, userID, roomID)
        {
            CreateCodec(videoType);
        }

        public virtual void CreateCodec(VideoCodecTypes videoType)
        {
            switch (videoType)
            {
                case VideoCodecTypes.Jpeg:
                    Codec = new JpegCodec();
                    break;
                case VideoCodecTypes.H264:
                    Codec = new H264Codec();
                    break;
                case VideoCodecTypes.H265:
                    Codec = new H265Codec();
                    break;
            }
        }

        public virtual void Init(VideoSource source)
        {
            Source = source;
        }

        public override bool Start()
        {
            if (Client == null)
            {
                throw new Exception("ارتباط مشخص نشده است");
            }
            if (Client.Channel == null)
            {
                throw new Exception("کانال ارتباطی مشخص نشده است");
            }
            ListenToSideEvents();
            return true;
        }
        public override void ListenToSideEvents()
        {
            //switch (DistributeSide)
            //{
            //    case DistributeSides.Receiver:
            //        IChannel channel = Client.Channel;
            //        channel.DataReceived += Channel_DataReceived;
            //        if (Client.Channel.Closed)
            //            Client.Start();
            //        break;

            Source.NewDataReceived += Source_NewDataReceived;
            Source.Start();
        }

        private void Source_NewDataReceived(object sender, EventArgs e)
        {
            byte[] frame = Source.CurrentData;
            if (VideoFrameFromSourceReceived != null)
                VideoFrameFromSourceReceived(this, this.Client, frame);
            Client.Send(CreatePackage(Codec.Encode(frame)));
            Source.GetNext();
            //RaiseFrameReceived(e.Frame);
        }

        //private void Channel_DataReceived(object sender, INetClient client, byte[] data)
        //{
        //    //------------------------ Processing Data as Package ------------------------------
        //    Package pak = Package.FromByteArray(data);
        //    ParsePackage(pak);
        //    //----------------------------------------------------------------------------------
        //}
        //private void ParsePackage(Package pak)
        //{
        //    if (pak.UserID != UserID)
        //        return;
        //    try
        //    {
        //        switch (pak.CommandType)
        //        {
        //            case CommandType.VideoBroadcast:
        //                byte[] frame = (byte[])Codec.Decode(pak.Data);
        //                RaiseFrameReceived(frame);
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
                
        //    }
        //}

        private byte[] CreatePackage(byte[] frame)
        {
            Package pak = new Package(CommandType.VideoBroadcast, System.Net.IPAddress.Broadcast, Client.IP, null, RoomID);
            pak.UserID = Client.UserID;
            pak.SenderIP = Client.IP;
            pak.Data = frame;
            return Package.ToByteArray(pak);
        }
    }
}


