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
    public class Client
    {
        private readonly Protocol _protocol;
        //private readonly Channel clientToServer;
        //private readonly Channel serverToClient;
        readonly Stream _stream;
        private Transport _transport;
        public Client(Protocol protocol, Stream stream)
        {
            _protocol = protocol;
            //clientToServer = output;
            //serverToClient = input;
            _stream = stream;
        }
        public async Task Handshake(byte[] privateKey, byte[] serverPublicKey)
        {
            var buffer = new byte[Protocol.MaxMessageLength];
      

            using (var handshakeState = _protocol.Create(true, s: privateKey, rs: serverPublicKey))
            {
                // Send the first handshake message to the server.
                var (bytesWritten, _, _) = handshakeState.WriteMessage(null, buffer);
                //await clientToServer.Send(buffer.Slice(bytesWritten));
                await _stream.WriteAsync(buffer.Slice(bytesWritten));
  
                // Receive the second handshake message from the server.
                //var received = await serverToClient.Receive();
                var received = new byte[1024];
                var bytesRead = await _stream.ReadAsync(received, 0, 1024);
                var (_, _, transport) = handshakeState.ReadMessage(received.Slice(bytesRead), buffer);
                _transport = transport;
               
            }
        }
        public async Task SendMessages(List<string> messages)
        {
            var buffer = new byte[Protocol.MaxMessageLength];

            // Handshake complete, switch to transport mode.
            foreach (var message in messages)
            {
                Memory<byte> request = Encoding.UTF8.GetBytes(message);
                Console.WriteLine($"message size: {request.Length}");

                // Send the message to the server.
                var bytesWritten = _transport.WriteMessage(request.Span, buffer);
                //await clientToServer.Send(buffer.Slice(bytesWritten));
                await _stream.WriteAsync(buffer.Slice(bytesWritten));

                // Receive the response and print it to the standard output.
                //var response = await serverToClient.Receive();
                var received = new byte[1024];
                var bytesRead = await _stream.ReadAsync(received, 0, 1024);
                bytesRead = _transport.ReadMessage(received.Slice(bytesRead), buffer);

                Console.WriteLine(Encoding.UTF8.GetString(buffer.Slice(bytesRead)));
            }
        }
    }
}