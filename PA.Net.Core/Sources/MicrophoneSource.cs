using Alvas.Audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace PA.Net.Core.Sources
{
    public class MicrophoneSource : AudioSource
    {
        private RecorderEx soundSource = null;
        private bool requestedToStop;
        private ManualResetEvent fManualResetEvent = new ManualResetEvent(false);
        private byte[] currentFrame;

        private WaveReader wr = null;
        private WaveWriter ww = null;
        private Stream _stream = null;

        public Stack<byte[]> FrameStack = new Stack<byte[]>();
        private bool freeBuffer = true;

        public int RecorderID
        {
            get { return soundSource.RecorderID; }
            set { soundSource.RecorderID = value; }
        }

        public IntPtr Format
        {
            get { return soundSource.Format; }
            set { soundSource.Format = value; }
        }

        public void InitializeDevice(int deviceID,FormatDetails formatDtl)
        {
            try
            {
                this.soundSource = new RecorderEx();
                RecorderID = deviceID;
                soundSource.Format = formatDtl.FormatHandle;
                // recEx
                // 
                this.soundSource.Close += new EventHandler(CloseRecorder);
                this.soundSource.Data += new Alvas.Audio.RecorderEx.DataEventHandler(DataRecorder);
                this.soundSource.Open += new EventHandler(OpenRecorder);
            }
            catch (ApplicationException)
            {
                throw new Exception("میکروفن شناسایی نشد!!!");
            }
        }

        private void OpenRecorder(object sender, EventArgs e)
        {
            if (ww != null)
            {
                ww.Close();
            }
            ww = new WaveWriter(_stream, soundSource.FormatBytes());
        }

        private void DataRecorder(object sender, DataEventArgs e)
        {
            if (!freeBuffer)
                return;
            if (requestedToStop)
                return;
            if (!requestedToStop)
            {
                byte[] data = e.Data;
                ww.WriteData(data);
                long pos = soundSource.GetPosition(TimeFormat.Milliseconds);
                // OnChangePosition(pos);
                CurrentData = data;
                currentFrame = CurrentData;
                lock (FrameStack)
                {
                    FrameStack.Push(currentFrame);

                }

                RaiseNewDataReceived();


            }
        }

        private void CloseRecorder(object sender, EventArgs e)
        {

        }

        //public MicrophoneSource()
        //{
        //    DriverDetails[] drvs = AudioCompressionManager.GetDriverList();
        //    FormatTagDetails[] tags = AudioCompressionManager.GetFormatTagList(drvs[0].Driver);
        //    FormatDetails[] frms = AudioCompressionManager.GetFormatList(tags[0].FormatTag);
        //    InitializeDevice(0, frms[0]);
        //    //   InitializeDevice(device);
        //}

        //public MicrophoneSource(int deviceID)
        //{
        //    FormatTagDetails[] tags = AudioCompressionManager.GetFormatTagList(deviceID);
        //    FormatDetails[] frms = AudioCompressionManager.GetFormatList(tags[0].FormatTag);
        //    InitializeDevice(deviceID, frms[0]);
        //}

        public MicrophoneSource(int deviceID, FormatDetails frmt)
        {
            InitializeDevice(deviceID, frmt);
        }

        public override void Start()
        {
            base.Start();
            requestedToStop = false;

            if (soundSource != null)
            {
                if (soundSource.State == DeviceState.Closed)
                {
                    _stream = new MemoryStream();
                }
                soundSource.StartRecord();
            }
        } 

        public override void Stop()
        {
            base.Stop();
            requestedToStop = true;
            soundSource.StopRecord();
        }

        public override void GetNext()
        {
            freeBuffer = true;
            FrameStack.Clear();
        }
    }
}
