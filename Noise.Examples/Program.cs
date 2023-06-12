using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Noise.Examples
{
    public class Program
    {
        // Noise_IKpsk2_25519_ChaChaPoly_BLAKE2b
        private static readonly Protocol protocol = new Protocol(
            HandshakePattern.KK,
            CipherFunction.ChaChaPoly,
            HashFunction.Blake2b,
            PatternModifiers.None
        );

        private static readonly List<string> messages = new List<string>
        {
            "Now that the party is jumping",
            "With the bass kicked in, the fingers are pumpin'",
            "Quick to the point, to the point no faking",
            "I'm cooking MC's like a pound of bacon"
        };

        public static KeyPair GenerateKeyPair()
        {
            var key = KeyPair.Generate();
            Console.WriteLine(Convert.ToHexString(key.PrivateKey));
            Console.WriteLine(Convert.ToHexString(key.PublicKey));
            return key;

        }

        public static void Main(string[] args)
        {
            var duplex = new Duplex();

            var clientPrivateKey = Convert.FromHexString("234D30626FA21534A56A7DDF825357DD3637E1BFBB8C4FB714BE935C8795655D");
            var clientPublicKey = Convert.FromHexString("FA0DBBF96EE194CD49325A7AEFD52EABE6D638A17A60CA4796C4F08A92221F6C");
            var serverPrivateKey = Convert.FromHexString("B321CA343B77CFF728F6F5346F360E54CE4C3CB50ACDE0C4D7EF88CC288C14C6");
            var serverPublicKey = Convert.FromHexString("704A34B610576F037D44E53DF80D52B40307ECC04523DA06BE8599DB111B6523");
#if false
            // Initialize and run the client.
            var clientTask = Task.Run(async () =>
            {
                using TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 7001);

                using NetworkStream netStream = tcpClient.GetStream();

                var client = new Client(protocol, netStream);
                await client.Handshake(clientPrivateKey, serverPublicKey);
                await client.SendMessages(messages);
            });
#endif
            // Initialize and run the server.
            var serverTask = Task.Run(async () =>
            {
                try
                {
                    TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 7001);
                    tcpListener.Start();  // this will start the server
                    TcpClient client = tcpListener.AcceptTcpClient();  //if a connection exists, the server will accept it
                    Console.WriteLine("here");
                    NetworkStream ns = client.GetStream();

                    var server = new Server(protocol, ns);
                    await server.Handshake(serverPrivateKey, clientPublicKey);
                    await server.WaitMessages();
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            });

            try
            {
                Task.WaitAll(serverTask);

            }
            catch (System.Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }


        }
    }
}
