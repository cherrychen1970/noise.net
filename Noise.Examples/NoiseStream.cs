using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Noise;

namespace Noise.Examples
{

    public class NoiseStream : System.IO.Stream
    {
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override long Length { get => throw new NotImplementedException(); }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        private readonly Stream _stream;
        private readonly MessageStream _messageStream;
        private readonly Transport _transport;
        public NoiseStream(Stream s, Transport t)
        {
            _stream = s;
            _messageStream = new MessageStream(s);
            _transport = t;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("use async");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("use async");
        }

        // TODO : AEAD size 16? check this assumption
        const int NoiseAeadTagsize = 16;
        byte[] noiseReadBuffer = new byte[Protocol.MaxMessageLength];
        async public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var bytesRead = await _messageStream.ReadMessage(noiseReadBuffer);//, 0, noiseBufferSize, cancellationToken);
            if (bytesRead < 1)
                throw new IOException("NoiseStream read failed");

            return _transport.ReadMessage(noiseReadBuffer.Slice(bytesRead), buffer);
        }
        byte[] noiseWriteBuffer = new byte[Protocol.MaxMessageLength];
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var bytesWritten = _transport.WriteMessage(buffer, noiseWriteBuffer);
            return _messageStream.SendMessage(noiseWriteBuffer, bytesWritten);
        }

        public override bool CanWrite => _stream.CanWrite;
        public override bool CanRead => _stream.CanRead;
        public override void Flush()
        {
            _stream.Flush();
        }
        public override bool CanSeek => _stream.CanSeek;
    }

}