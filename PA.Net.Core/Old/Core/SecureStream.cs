using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PA.Net.Core
{
    public class SecureStream : Stream
    {
        private static readonly byte[] fEmptyReadBuffer = new byte[0];

        private MemoryStream fWriteBuffer;

        /// <summary>
        /// Creates a new secure stream (stre  am that uses an assymetric key to
        /// initialize and then a symmetric key to continue it's work) over another
        /// stream, without any other parameters, so, running as client.
        /// </summary>
        public SecureStream(Stream baseStream) :
            this(baseStream, new RSACryptoServiceProvider(), SymmetricAlgorithm.Create(), false)
        {
        }

        /// <summary>
        /// Creates a new secure stream (stream that uses an assymetric key to
        /// initialize and then a symmetric key to continue it's work) over another
        /// stream, specifying if running as client or server, but without changing
        /// the default symmetric or assymetric class/algorithm..
        /// </summary>
        public SecureStream(Stream baseStream, bool runAsServer) :
            this(baseStream, new RSACryptoServiceProvider(), SymmetricAlgorithm.Create(), runAsServer)
        {
        }

        /// <summary>
        /// Creates a new secure stream (stream that uses an assymetric key to
        /// initialize and then a symmetric key to continue it's work) over another
        /// stream. <br/>
        /// Species the symmetricAlgorithm to use.
        /// </summary>
        public SecureStream(Stream baseStream, SymmetricAlgorithm symmetricAlgorithm) :
            this(baseStream, new RSACryptoServiceProvider(), symmetricAlgorithm, false, 2048)
        {
        }

        /// <summary>
        /// Creates a new secure stream (stream that uses an assymetric key to
        /// initialize and then a symmetric key to continue it's work) over another
        /// stream. <br/>
        /// Species the symmetricAlgorithm to use and if it runs as a client or server.
        /// </summary>
        public SecureStream(Stream baseStream, SymmetricAlgorithm symmetricAlgorithm, bool runAsServer) :
            this(baseStream, new RSACryptoServiceProvider(), symmetricAlgorithm, runAsServer, 2048)
        {
        }

        /// <summary>
        /// Creates a new secure stream (stream that uses an assymetric key to
        /// initialize and then a symmetric key to continue it's work) over another
        /// stream. <br/>
        /// Specifies the assymetric and the symmetric algorithm to use, and if it 
        /// must run as client or server.
        /// </summary>
        public SecureStream(Stream baseStream, RSACryptoServiceProvider rsa, SymmetricAlgorithm symmetricAlgorithm, bool runAsServer) :
            this(baseStream, rsa, symmetricAlgorithm, runAsServer, 2048)
        {
        }

        /// <summary>
        /// Creates a new secure stream (stream that uses an assymetric key to
        /// initialize and then a symmetric key to continue it's work) over another
        /// stream. <br/>
        /// Specifies the assymetric and the symmetric algorithm to use, if it 
        /// must run as client or server and the writeBufferInitialLength.
        /// </summary>
        public SecureStream(Stream baseStream, RSACryptoServiceProvider rsa, SymmetricAlgorithm symmetricAlgorithm, bool runAsServer, int writeBufferInitialLength)
        {
            if (baseStream == null)
                throw new ArgumentNullException("baseStream");

            if (rsa == null)
                throw new ArgumentNullException("rsa");

            if (symmetricAlgorithm == null)
                throw new ArgumentNullException("symmetricAlgorithm");

            if (writeBufferInitialLength < 0)
                throw new ArgumentException("writeBufferInitialLength must can't be negative.", "writeBufferInitialLength");

            BaseStream = baseStream;
            SymmetricAlgorithm = symmetricAlgorithm;

            string symmetricTypeName = symmetricAlgorithm.GetType().ToString();
            byte[] symmetricTypeBytes = Encoding.UTF8.GetBytes(symmetricTypeName);
            if (runAsServer)
            {
                byte[] sizeBytes = BitConverter.GetBytes(symmetricTypeBytes.Length);
                baseStream.Write(sizeBytes, 0, sizeBytes.Length);
                baseStream.Write(symmetricTypeBytes, 0, symmetricTypeBytes.Length);

                byte[] bytes = rsa.ExportCspBlob(false);
                sizeBytes = BitConverter.GetBytes(bytes.Length);
                baseStream.Write(sizeBytes, 0, sizeBytes.Length);
                baseStream.Write(bytes, 0, bytes.Length);

                symmetricAlgorithm.Key = p_ReadWithLength(rsa); ;
                symmetricAlgorithm.IV = p_ReadWithLength(rsa);
            }
            else
            {
                // ok. We run as the client, so first we first check the
                // algorithm types and then receive the assymetric
                // key from the server.

                // symmetricAlgorithm
                var sizeBytes = new byte[4];
                p_ReadDirect(sizeBytes);
                var stringLength = BitConverter.ToInt32(sizeBytes, 0);

                if (stringLength != symmetricTypeBytes.Length)
                    throw new ArgumentException("Server and client must use the same SymmetricAlgorithm class.");

                var stringBytes = new byte[stringLength];
                p_ReadDirect(stringBytes);
                var str = Encoding.UTF8.GetString(stringBytes);
                if (str != symmetricTypeName)
                    throw new ArgumentException("Server and client must use the same SymmetricAlgorithm class.");

                // public key.
                sizeBytes = new byte[4];
                p_ReadDirect(sizeBytes);
                int asymmetricKeyLength = BitConverter.ToInt32(sizeBytes, 0);
                byte[] bytes = new byte[asymmetricKeyLength];
                p_ReadDirect(bytes);
                rsa.ImportCspBlob(bytes);

                // Now that we have the asymmetricAlgorithm set, and considering
                // that the symmetricAlgorithm initializes automatically, we must
                // only send the key.
                p_WriteWithLength(rsa, symmetricAlgorithm.Key);
                p_WriteWithLength(rsa, symmetricAlgorithm.IV);
            }

            // After the object initialization being done, be it a client or a
            // server, we can dispose the assymetricAlgorithm.
            rsa.Clear();

            Decryptor = symmetricAlgorithm.CreateDecryptor();
            Encryptor = symmetricAlgorithm.CreateEncryptor();

            fReadBuffer = fEmptyReadBuffer;
            fWriteBuffer = new MemoryStream(writeBufferInitialLength);
        }

        /// <summary>
        /// Releases the buffers, the basestream and the cryptographic classes.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var writeBuffer = fWriteBuffer;
                if (writeBuffer != null)
                {
                    fWriteBuffer = null;
                    writeBuffer.Dispose();
                }

                var encryptor = this.Encryptor;
                if (encryptor != null)
                {
                    Encryptor = null;
                    encryptor.Dispose();
                }

                var decryptor = this.Decryptor;
                if (decryptor != null)
                {
                    Decryptor = null;
                    decryptor.Dispose();
                }

                var symmetricAlgorithm = SymmetricAlgorithm;
                if (symmetricAlgorithm != null)
                {
                    SymmetricAlgorithm = null;
                    symmetricAlgorithm.Clear();
                }

                var baseStream = this.BaseStream;
                if (baseStream != null)
                {
                    BaseStream = null;
                    baseStream.Dispose();
                }

                fReadBuffer = null;
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets the original stream that created this asymmetric crypto stream.
        /// </summary>
        public Stream BaseStream { get; private set; }

        /// <summary>
        /// Gets the symmetric algorithm being used.
        /// </summary>
        public SymmetricAlgorithm SymmetricAlgorithm { get; private set; }

        /// <summary>
        /// Gets the encryptor being used.
        /// </summary>
        public ICryptoTransform Decryptor { get; private set; }

        /// <summary>
        /// Gets the decryptor being used.
        /// </summary>
        public ICryptoTransform Encryptor { get; private set; }

        /// <summary>
        /// Always returns true.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Always returns false.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Always returns true.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Throws a NotSupportedException.
        /// </summary>
        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Throws a NotSupportedException.
        /// </summary>
        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        private readonly byte[] fSizeBytes = new byte[5];
        private int fReadPosition;
        private byte[] fReadBuffer;
        private byte fReadCRC;

        /// <summary>
        /// Reads and decryptographs the given number of bytes from the buffer.
        /// </summary>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (fReadPosition == fReadBuffer.Length)
            {
                p_ReadDirect(fSizeBytes);
                int readLength = BitConverter.ToInt32(fSizeBytes, 0);

                if (fReadBuffer.Length < readLength)
                    fReadBuffer = new byte[readLength];

                p_FullReadDirect(fReadBuffer, readLength);
                fReadBuffer = Decryptor.TransformFinalBlock(fReadBuffer, 0, readLength);

                // here we check if our actual CRC is the same as the message CRC.
                byte crc = fSizeBytes[4];
                if (crc != fReadCRC)
                    throw new IOException("Invalid CRC.");

                // And after we decrypt the message with such crc.
                int readBufferLength = fReadBuffer.Length;
                for (int i = 0; i < readBufferLength; i++)
                {
                    byte newCrc = fReadBuffer[i];
                    fReadBuffer[i] ^= crc;
                    crc = newCrc;
                }

                fReadCRC = crc;
                fReadPosition = 0;
            }

            int diff = fReadBuffer.Length - fReadPosition;
            if (count > diff)
                count = diff;

            Buffer.BlockCopy(fReadBuffer, fReadPosition, buffer, offset, count);
            fReadPosition += count;
            return count;
        }

        /// <summary>
        /// Throws a NotSupportedException.
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Throws a NotSupportedException.
        /// </summary>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Encrypts and writes the given bytes.
        /// </summary>
        public override void Write(byte[] buffer, int offset, int count)
        {
            fWriteBuffer.Write(buffer, offset, count);
        }

        private int fMaxLength;
        private int fCollectionNumber = GC.CollectionCount(GC.MaxGeneration);
        private byte fWriteCRC;
        /// <summary>
        /// Sends all the buffered data.
        /// </summary>
        public override void Flush()
        {
            int length = (int)fWriteBuffer.Length;
            if (length > 0)
            {
                // here we pre-encrypt the block and generate a CRC. We do this so
                // two identical blocks will, in fact, be different.
                var writeBuffer = fWriteBuffer.GetBuffer();
                byte crc = fWriteCRC;
                for (int i = 0; i < length; i++)
                {
                    crc ^= writeBuffer[i];
                    writeBuffer[i] = crc;
                }

                var encryptedBuffer = Encryptor.TransformFinalBlock(writeBuffer, 0, length);
                var size = BitConverter.GetBytes(encryptedBuffer.Length);
                BaseStream.Write(size, 0, size.Length);
                BaseStream.WriteByte(fWriteCRC);
                BaseStream.Write(encryptedBuffer, 0, encryptedBuffer.Length);
                BaseStream.Flush();

                fWriteCRC = crc;
                fWriteBuffer.SetLength(0);

                int collectionNumber = GC.CollectionCount(GC.MaxGeneration);
                if (collectionNumber == fCollectionNumber)
                {
                    if (length > fMaxLength)
                        fMaxLength = length;
                }
                else
                {
                    if (fMaxLength != 0)
                    {
                        int halfLength = fWriteBuffer.Capacity / 2;
                        if (fMaxLength < halfLength)
                            fWriteBuffer.Capacity = fMaxLength + (fMaxLength / 2);

                        fMaxLength = 0;
                    }

                    fCollectionNumber = collectionNumber;
                }
            }
        }

        private void p_ReadDirect(byte[] bytes)
        {
            p_FullReadDirect(bytes, bytes.Length);
        }
        private void p_FullReadDirect(byte[] bytes, int length)
        {
            int read = 0;
            while (read < length)
            {
                int readResult = BaseStream.Read(bytes, read, length - read);

                if (readResult == 0)
                    throw new IOException("The stream was closed by the remote side.");

                read += readResult;
            }
        }
        private byte[] p_ReadWithLength(RSACryptoServiceProvider rsa)
        {
            byte[] size = new byte[4];
            p_ReadDirect(size);

            int count = BitConverter.ToInt32(size, 0);
            var bytes = new byte[count];
            p_ReadDirect(bytes);

            return rsa.Decrypt(bytes, false);
        }
        private void p_WriteWithLength(RSACryptoServiceProvider rsa, byte[] bytes)
        {
            bytes = rsa.Encrypt(bytes, false);
            byte[] sizeBytes = BitConverter.GetBytes(bytes.Length);
            BaseStream.Write(sizeBytes, 0, sizeBytes.Length);
            BaseStream.Write(bytes, 0, bytes.Length);
        }
    }
}
