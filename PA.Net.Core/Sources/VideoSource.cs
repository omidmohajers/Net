namespace PA.Net.Core.Sources
{
    public class VideoSource : MediaSource
    {
        public byte FrameRate { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
    }
}
