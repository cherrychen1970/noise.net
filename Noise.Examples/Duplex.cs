using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

//https://learn.microsoft.com/en-us/dotnet/api/system.io.pipes.pipestream.read?view=net-7.0

namespace Noise
{
    class Duplex
    {
        public readonly DuplexStream A;
        public readonly DuplexStream B;

        public Duplex()
        {
            // A <--- B
            var inputA = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            var outputB = new AnonymousPipeClientStream(PipeDirection.Out, inputA.GetClientHandleAsString());

            // A ---> B
            var inputB = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            var outputA = new AnonymousPipeClientStream(PipeDirection.Out, inputB.GetClientHandleAsString());

            A = new DuplexStream(inputA, outputA);
            B = new DuplexStream(inputB, outputB);
        }
    }

    public class DuplexStream : System.IO.Stream
    {
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override long Length => _input.Length;
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        private readonly PipeStream _input;
        private readonly PipeStream _output;
        public DuplexStream(PipeStream input, PipeStream output)
        {
            _input = input;
            _output = output;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead= _input.Read(buffer, offset, count);
            Console.WriteLine($"read {bytesRead}");
            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
             _output.Write(buffer, offset, count);
             Console.WriteLine($"Write {count}");
        }

        public override bool CanWrite => _output.CanWrite;
        public override bool CanRead => _input.CanRead;
        public override void Flush()
        {
            _output.Flush();
        }
        public override bool CanSeek => _input.CanSeek;



        private async Task Send(byte[] message)
        {
            await Task.Delay(1000);
            await _output.WriteAsync(message);
        }

        private async Task<byte[]> Receive(int length)
        {
            byte[] buf = new byte[length];

            // TODO : probably should loop until you read them all
            int len = await _input.ReadAsync(buf, 0, length);

            if (len != length)
                throw new System.IO.IOException("read from thefailed");
            return buf;
        }
    }
}