using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Noise.Examples
{
    public class Server
    {
        private readonly Protocol _protocol;
        //private readonly Channel clientToServer;
        //private readonly Channel serverToClient;
        DuplexStream _stream;
        Transport _transport;
        public Server(Protocol protocol, DuplexStream stream)
        {
            _protocol = protocol;
            //clientToServer = input;
            //serverToClient = output;
            _stream = stream;
        }
        public async Task Handshake(byte[] privateKey, byte[] clientPublicKey)
        {
            var buffer = new byte[Protocol.MaxMessageLength];

            using (var handshakeState = _protocol.Create(false, s: privateKey, rs: clientPublicKey))
            {
                // Receive the first handshake message from the client.
                //var received = await clientToServer.Receive();
                var received = new byte[1024];
                var bytesRead = await _stream.ReadAsync(received, 0, 1024);
                handshakeState.ReadMessage(received.Slice(bytesRead), buffer);
                //throw new Exception("abc");

                // Send the second handshake message to the client.
                var (bytesWritten, _, transport) = handshakeState.WriteMessage(null, buffer);
                _transport = transport;
                //await serverToClient.Send(buffer.Slice(bytesWritten));
                await _stream.WriteAsync(buffer.Slice(bytesWritten));
            }
        }
        public async Task WaitMessages()
        {
            var buffer = new byte[Protocol.MaxMessageLength];

            for (; ; )
            {
                // Receive the message from the client.
                //var request = await clientToServer.Receive();
                var received = new byte[1024];
                var bytesRead = await _stream.ReadAsync(received, 0, 1024);

                bytesRead = _transport.ReadMessage(received.Slice(bytesRead), buffer);

                // Echo the message back to the client.
                var bytesWritten = _transport.WriteMessage(buffer.Slice(bytesRead), buffer);
                //await serverToClient.Send(buffer.Slice(bytesWritten));
                await _stream.WriteAsync(buffer.Slice(bytesWritten));
            }
        }
    }
}