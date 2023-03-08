using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace PA.Net.Core.Sources
{
    public class WebcamSource : VideoSource
    {
        private VideoCaptureDevice videoSource = null;
        private bool requestedToStop;
        private ManualResetEvent fManualResetEvent = new ManualResetEvent(false);
        private Bitmap convertedFrame;
        private Bitmap currentFrame;

        public Stack<byte[]> FrameStack = new Stack<byte[]>();
        private bool freeBuffer = true;

        public string SelectDevice(int index)
        {
            try
            {
                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                if (videoDevices.Count == 0)
                    throw new Exception("وب کم شناسایی نشد");
                else
                    return videoDevices[index].MonikerString;
            }
            catch (ApplicationException)
            {
                throw new Exception("وب کم شناسایی نشد!!!");
            }
        }

        public WebcamSource(int index)
        {
            string device = SelectDevice(index);
            InitializeDevice(device);
        }

        public WebcamSource(string device)
        {
            InitializeDevice(device);
        }

        public void InitializeDevice(VideoCaptureDevice device)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }
            videoSource = device;
            videoSource.NewFrame += VideoSource_NewFrame;
        }

        public void InitializeDevice(string device)
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
            }
            videoSource = new VideoCaptureDevice(device);
            videoSource.NewFrame += VideoSource_NewFrame;
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs e)
        {
            if (!freeBuffer)
                return;
            if (requestedToStop)
                return;
            if (!requestedToStop)
            {
                Bitmap newFrame = (Bitmap)e.Frame.Clone();
                currentFrame = newFrame;
                // check if conversion is required to lower bpp rate
                if ((currentFrame.PixelFormat == PixelFormat.Format16bppGrayScale) ||
                     (currentFrame.PixelFormat == PixelFormat.Format48bppRgb) ||
                     (currentFrame.PixelFormat == PixelFormat.Format64bppArgb))
                {
                    convertedFrame = AForge.Imaging.Image.Convert16bppTo8bpp(currentFrame);
                }
                else
                {
                    convertedFrame = currentFrame;
                }
                currentFrame = convertedFrame;
                using (var stream = new MemoryStream())
                {
                    currentFrame.Save(stream, ImageFormat.Jpeg);
                    CurrentData = stream.ToArray();
                    lock (FrameStack)
                    {
                        FrameStack.Push(stream.ToArray());

                    }
                }
                RaiseNewDataReceived();
            }
        }

        public override void Start()
        {
            base.Start();
            requestedToStop = false;

            if (videoSource != null)
            {
                videoSource.Start();
            }

        }

        public override void Stop()
        {
            base.Stop();
            requestedToStop = true;
            videoSource.SignalToStop();
        }

        public override void GetNext()
        {
            freeBuffer = true;
            FrameStack.Clear();
        }
    }
}
