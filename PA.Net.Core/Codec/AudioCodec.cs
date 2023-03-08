using Alvas.Audio;
using System;
namespace PA.Codec
{
    public class AudioCodec : ICodec
    {
        public AudioCodec(FormatDetails audioType)
        {
            FormatDetails =  (FormatDetails)audioType;
            Format = FormatDetails.FormatHandle;
        }

        public FormatTagDetails FormatTagDetails { get; set; }
        public FormatDetails FormatDetails { get; set; }
        public IntPtr Format { get; private set; }
        public string CodecName
        {
            get
            {
                return FormatTagDetails.FormatTagName;
            }
        }

        public virtual byte[] Encode(object data)
        {
            byte[] d = data as byte[];
            short[] buffer = AudioCompressionManager.RecalculateData(Format, d, d.Length);
            var currentFrame = new byte[buffer.Length * 2];
            Buffer.BlockCopy(buffer, 0, currentFrame, 0, buffer.Length * 2);
            return currentFrame;
        }

        public virtual object Decode(byte[] data)
        {
            // Convert byte[] to short[]
            short[] target = new short[data.Length / 2];

            Buffer.BlockCopy(data, 0, target, 0, data.Length);



            return target;
        }

    }
}


