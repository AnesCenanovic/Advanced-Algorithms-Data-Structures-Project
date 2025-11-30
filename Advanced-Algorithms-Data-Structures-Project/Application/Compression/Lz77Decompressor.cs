namespace Application.Compression
{
    public class Lz77Decompressor : ILz77Decompressor
    {
        public byte[] Decompress(byte[] input)
        {
            List<byte> output = new List<byte>();

            using (var inputStream = new MemoryStream(input))
            using (var reader = new BinaryReader(inputStream))
            {
                // Read until end of stream
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // Read Token (4 bytes total)
                    short offset = reader.ReadInt16();
                    byte length = reader.ReadByte();
                    byte nextByte = reader.ReadByte();

                    if (length > 0)
                    {
                        // Copy from history
                        int startCopyIndex = output.Count - offset;

                        // Safety check for corrupt files
                        if (startCopyIndex < 0) startCopyIndex = 0;

                        for (int i = 0; i < length; i++)
                        {
                            if (startCopyIndex + i < output.Count)
                            {
                                output.Add(output[startCopyIndex + i]);
                            }
                        }
                    }

                    // Append the literal byte
                    output.Add(nextByte);
                }
            }

            return output.ToArray();
        }
    }
}