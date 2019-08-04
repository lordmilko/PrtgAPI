using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using PrtgAPI.Utilities;

namespace PrtgAPI.Request.Serialization
{
    class SensorHistoryStream : Stream
    {
        Stream stream;
        bool hasRead;

        public SensorHistoryStream(Stream stream)
        {
            this.stream = stream;
        }

        private int ReadInternal(byte[] buffer, int offset, int count)
        {
            var result = stream.Read(buffer, offset, count);

            if (!hasRead)
            {
                bool isValid = false;

                for(var i = 0; i < buffer.Length; i++)
                {
                    if ((char)buffer[i] == '<')
                    {
                        isValid = true;
                    }
                }

                if (!isValid)
                {
                    var message = Encoding.UTF8.GetString(buffer);

                    throw new PrtgRequestException($"PRTG was unable to complete the request. The server responded with the following error: {message.EnsurePeriod()}");
                }

                hasRead = true;
            }

            return result;
        }

        #region Stream

        [ExcludeFromCodeCoverage]
        public override bool CanRead => stream.CanRead;

        [ExcludeFromCodeCoverage]
        public override bool CanSeek => stream.CanSeek;

        [ExcludeFromCodeCoverage]
        public override bool CanWrite => stream.CanWrite;

        [ExcludeFromCodeCoverage]
        public override long Length => stream.Length;

        [ExcludeFromCodeCoverage]
        public override long Position
        {
            get { return stream.Position; }

            set { stream.Position = value; }
        }

        [ExcludeFromCodeCoverage]
        public override void Flush() => stream.Flush();

        [ExcludeFromCodeCoverage]
        public override int Read(byte[] buffer, int offset, int count) => ReadInternal(buffer, offset, count);

        [ExcludeFromCodeCoverage]
        public override long Seek(long offset, SeekOrigin origin) => stream.Seek(offset, origin);

        [ExcludeFromCodeCoverage]
        public override void SetLength(long value) => stream.SetLength(value);

        [ExcludeFromCodeCoverage]
        public override void Write(byte[] buffer, int offset, int count) => stream.Write(buffer, offset, count);

        #endregion
    }
}
