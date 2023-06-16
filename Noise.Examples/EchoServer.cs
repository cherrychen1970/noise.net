using System;
using System.IO;
using System.Threading.Tasks;
namespace Noise.Examples
{
    public class EchoServer
    {

        Stream _stream;


        public EchoServer(Stream stream)
        {
            _stream = stream;
        }

        public async Task WaitMessages()
        {
            var received = new byte[Protocol.MaxMessageLength];
            for (; ; )
            {
                int bytesRead = await _stream.ReadAsync(received);
                await _stream.WriteAsync(received,0, bytesRead);
            }
        }
    }
}