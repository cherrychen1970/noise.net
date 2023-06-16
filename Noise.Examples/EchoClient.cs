using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Noise.Examples
{
    public class EchoClient
    {
        Stream _stream;
        byte[] received = new byte[1024];

        public EchoClient(Stream stream)
        {
            _stream = stream;
        }

        public async Task Run()
        {
            while (true)
            {
                var message = System.Console.ReadLine();
                var request = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(request);

                received.Initialize();
                var bytesRead = await _stream.ReadAsync(received, 0, 1024);


                Console.WriteLine($"Received: {bytesRead}" + Encoding.UTF8.GetString(received.Slice(bytesRead)));
            }
        }
    }
}