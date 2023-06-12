using System;
using System.IO;
using System.Threading.Tasks;
namespace Noise.Examples
{
    public class Server
    {
        private readonly Protocol _protocol;
        //private readonly Channel clientToServer;
        //private readonly Channel serverToClient;
        Stream _stream;
        Transport _transport;
        public Server(Protocol protocol, Stream stream)
        {
            _protocol = protocol;
            //clientToServer = input;
            //serverToClient = output;
            _stream = stream;
        }
        public async Task Handshake(byte[] privateKey, byte[] clientPublicKey)
        {
            var buffer = new byte[Protocol.MaxMessageLength];
            var received = new byte[Protocol.MaxMessageLength];

            using (var handshakeState = _protocol.Create(false, s: privateKey, rs: clientPublicKey))
            {
                // Receive the first handshake message from the client.
                var bytesRead = await ReadMessage(received);
                handshakeState.ReadMessage(received.Slice(bytesRead), buffer);

                // Send the second handshake message to the client.
                var (bytesWritten, _, transport) = handshakeState.WriteMessage(null, buffer);
                _transport = transport;
                await SendMessage(buffer, bytesWritten);
            }
        }

        async private Task<int> ReadMessage(byte[] received)
        {
            byte[] lenBytes = new byte[2];
            var bytesRead = await _stream.ReadAsync(lenBytes, 0, 2);
            if (2 != bytesRead)
                throw new IOException("read failed");
            Array.Reverse(lenBytes);
            var len = BitConverter.ToUInt16(lenBytes, 0);
            Console.WriteLine(len);

            bytesRead = await _stream.ReadAsync(received, 0, len);
            if (len != bytesRead)
                throw new IOException("read failed");
            Console.WriteLine($"{bytesRead}");
            return bytesRead;
        }
        async private Task SendMessage(byte[] data, int len)
        {
            var lenBytes = BitConverter.GetBytes((UInt16)len);
            Array.Reverse(lenBytes);

            var writeBuffer = new MemoryStream();
            writeBuffer.Write(lenBytes);
            writeBuffer.Write(data, 0, len);

            await _stream.WriteAsync(writeBuffer.ToArray());
        }
        public async Task WaitMessages()
        {
            var decrypted = new byte[Protocol.MaxMessageLength];
            var received = new byte[Protocol.MaxMessageLength];

            for (; ; )
            {
                int bytesRead = await ReadMessage(received);
                bytesRead = _transport.ReadMessage(received.Slice(bytesRead), decrypted);

                // Echo the message back to the client.
                var bytesWritten = _transport.WriteMessage(decrypted.Slice(bytesRead), decrypted);
                await SendMessage(decrypted, bytesWritten);
            }
        }
    }
}