using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Noise.Examples
{
    public static class BufferExtension
    {
        public static IEnumerable<T> Singleton<T>(T item)
        {
            yield return item;
        }

        public static byte[] Slice(this byte[] array, int length)
        {
            return array.AsSpan(0, length).ToArray();
        }

    }
    public class NoiseClient
    {
        private readonly Protocol _protocol;

        readonly MessageStream _stream;
        private Transport _transport;
        public NoiseClient(Protocol protocol, Stream stream)
        {
            _protocol = protocol;
            _stream = new MessageStream(stream);
        }
        public async Task Handshake(byte[] privateKey, byte[] serverPublicKey)
        {
            var buffer = new byte[Protocol.MaxMessageLength];

            using (var handshakeState = _protocol.Create(true, s: privateKey, rs: serverPublicKey))
            {
                // Send the first handshake message to the server.
                var (bytesWritten, _, _) = handshakeState.WriteMessage(null, buffer);
                //await clientToServer.Send(buffer.Slice(bytesWritten));
                await _stream.SendMessage(buffer.Slice(bytesWritten));

                // Receive the second handshake message from the server.
                //var received = await serverToClient.Receive();
                var received = new byte[1024];
                var bytesRead = await _stream.ReadMessage(received, 1024);
                var (_, _, transport) = handshakeState.ReadMessage(received.Slice(bytesRead), buffer);
                _transport = transport;

            }
        }
    }
}