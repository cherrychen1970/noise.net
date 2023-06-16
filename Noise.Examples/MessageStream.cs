using System;
using System.IO;
using System.Threading.Tasks;
namespace Noise.Examples
{
    public class MessageStream
    {
        Stream _stream;

        public MessageStream(Stream stream)
        {
            _stream = stream;
        }

        async public Task<int> ReadMessage(byte[] received, int? maxLength = null)
        {
            byte[] lenBytes = new byte[2];
            var bytesRead = await _stream.ReadAsync(lenBytes, 0, 2);
  
            if (2 != bytesRead)
                throw new IOException("read failed");
            Array.Reverse(lenBytes);
            var len = BitConverter.ToUInt16(lenBytes, 0);
                      Console.WriteLine($"ReadMessage: {len}");
            if (len > maxLength)
                throw new IOException("message length too big");
            bytesRead = await _stream.ReadAsync(received, 0, len);
            if (len != bytesRead)
                throw new IOException("read failed");
            Console.WriteLine($"{bytesRead}");
            return bytesRead;
        }
        async public Task SendMessage(byte[] data, int? len = null)
        {
            var realLen = len.HasValue ? len.Value : data.Length;
            var lenBytes = BitConverter.GetBytes((UInt16)(realLen));
            Array.Reverse(lenBytes);

            var writeBuffer = new MemoryStream();
            writeBuffer.Write(lenBytes);
            writeBuffer.Write(data, 0, realLen);

            await _stream.WriteAsync(writeBuffer.ToArray());
            Console.WriteLine($"SendMessage: {realLen}");
        }
    }
}