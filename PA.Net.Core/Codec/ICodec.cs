﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
namespace PA.Codec
{
    public interface ICodec
    {
        string CodecName { get; }

        byte[] Encode(object data);

        object Decode(byte[] data);

    }
}


