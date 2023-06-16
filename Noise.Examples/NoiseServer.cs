using System;
using System.IO;
using System.Threading.Tasks;
namespace Noise.Examples
{
    public class NoiseServer
    {
        private readonly Protocol _protocol;
        MessageStream _messageStream;
        Stream _stream;
        Transport _transport;

        public NoiseStream GetNoiseStream() {
            return new NoiseStream(_stream,_transport);
        }

        public NoiseServer(Protocol protocol, Stream stream)
        {
            _protocol = protocol;
            _stream = stream;
            _messageStream = new MessageStream(stream);

        }
        public async Task Handshake(byte[] privateKey, byte[] clientPublicKey)
        {
            var buffer = new byte[Protocol.MaxMessageLength];
            var received = new byte[Protocol.MaxMessageLength];

            using (var handshakeState = _protocol.Create(false, s: privateKey, rs: clientPublicKey))
            {
                // Receive the first handshake message from the client.
                var bytesRead = await _messageStream.ReadMessage(received);
                handshakeState.ReadMessage(received.Slice(bytesRead), buffer);

                // Send the second handshake message to the client.
                var (bytesWritten, _, transport) = handshakeState.WriteMessage(null, buffer);
                _transport = transport;
                await _messageStream.SendMessage(buffer, bytesWritten);
            }
        }


    }
}