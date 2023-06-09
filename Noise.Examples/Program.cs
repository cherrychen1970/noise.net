using System;
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

        public static void Main(string[] args)
        {
            //Channel clientToServer = new Channel();
            //Channel serverToClient = new Channel();
			var duplex = new Duplex();
            // Generate static keys for the client and the server.
            var clientStatic = KeyPair.Generate();
            var serverStatic = KeyPair.Generate();

            // Initialize and run the client.
            var clientTask = Task.Run(async () =>
            {
                var client = new Client(protocol, duplex.A);
                await client.Handshake(clientStatic.PrivateKey, serverStatic.PublicKey);
                await client.SendMessages(messages);
            });

            // Initialize and run the server.
            var serverTask = Task.Run(async () =>
            {
				try
				{
					                var server = new Server(protocol, duplex.B);
                await server.Handshake(serverStatic.PrivateKey, clientStatic.PublicKey);
                await server.WaitMessages();
				}
				catch (System.Exception ex)
				{
					
					Console.WriteLine(ex.ToString());
				}

            });

			try
			{
				Task.WaitAll(serverTask,clientTask);
				
			}
			catch (System.Exception ex)
			{
				
				Console.WriteLine(ex.ToString());
			}
			

        }
    }
}
