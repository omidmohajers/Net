using Alvas.Audio;
using PA.Codec;
using PA.Crypto;
using PA.Net.Clients;
using System;

namespace PA.Net.Core.Receivers
{
    public class AudioReceiver : MediaReceiver
    {
        public event DataReceiveHandler AudioFrameFromSourceReceived = null;
        public AudioReceiver(INetClient client, long userID, long roomID) : base(client, userID, roomID)
        {

        }
        public AudioReceiver(INetClient client, FormatDetails audioCodec, long userID, long roomID) : this(client, userID, roomID)
        {
            CreateCodec(audioCodec);
        }

        public virtual void CreateCodec(FormatDetails audioType)
        {
            Codec = new AudioCodec(audioType);
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
            IChannel channel = Client.Channel;
            channel.DataReceived += Channel_DataReceived;
            if (Client.Channel.Closed)
                Client.Start();
        }

        private void Channel_DataReceived(object sender, INetClient client, byte[] data)
        {
            //------------------------ Processing Data as Package ------------------------------
            Package pak = Package.FromByteArray(data);
            ParsePackage(pak);
            //----------------------------------------------------------------------------------
        }
        private void ParsePackage(Package pak)
        {
            if (pak.UserID != UserID)
                return;
            try
            {
                switch (pak.CommandType)
                {
                    case CommandType.VideoBroadcast:
                        byte[] frame = (byte[])Codec.Decode(pak.Data);
                        RaiseFrameReceived(frame);
                        break;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void RaiseFrameReceived(byte[] frame)
        {
            if (AudioFrameFromSourceReceived != null)
                AudioFrameFromSourceReceived(this, Client, frame);
        }
    }
}
