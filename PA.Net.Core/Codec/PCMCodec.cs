using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA.Codec
{
    public class PCMCodec : AudioCodec
    {
        public override byte[] Encode(object data)
        {
            return base.Encode(data);
        }

        public override object Decode(byte[] data)
        {
            return base.Decode(data);
        }
    }

    public class ALawCodec : AudioCodec
    {
        public override byte[] Encode(object data)
        {
            return base.Encode(data);
        }

        public override object Decode(byte[] data)
        {
            return base.Decode(data);
        }
    }

    public class ULawCodec : AudioCodec
    {
        public override byte[] Encode(object data)
        {
            return base.Encode(data);
        }

        public override object Decode(byte[] data)
        {
            return base.Decode(data);
        }
    }
}
