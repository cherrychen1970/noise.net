using System;
using System.IO;
using System.Threading.Tasks;
namespace Noise.Examples
{
    public class EchoServer
    {
        Stream _stream;
        byte[] buffer = new byte[Protocol.MaxMessageLength];
        public EchoServer(Stream stream)
        {
            _stream = stream;
        }

        public async Task Run()
        {
            while (true)
            {
                int bytesRead = await _stream.ReadAsync(buffer);
                Console.WriteLine($"Received: {bytesRead}");
                await _stream.WriteAsync(buffer, 0, bytesRead);
            }
        }
    }
}