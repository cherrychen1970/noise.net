using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Noise.Examples
{
    // Chanel simulates the network between the client and the server.
    public class Channel
    {
        private readonly BufferBlock<byte[]> buffer = new BufferBlock<byte[]>();

        public async Task Send(byte[] message)
        {
            await buffer.SendAsync(message);
        }

        public async Task<byte[]> Receive()
        {
            return await buffer.ReceiveAsync();
        }
    }
}