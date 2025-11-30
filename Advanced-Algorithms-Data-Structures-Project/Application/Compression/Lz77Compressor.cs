namespace Application.Compression
{
    public class Lz77Compressor : ILz77Compressor
    {
        public Lz77Result Compress(byte[] input, int windowSize, int lookaheadSize)
        {
            using (var outputStream = new MemoryStream())
            using (var writer = new BinaryWriter(outputStream))
            {
                int cursor = 0;
                int tokenCount = 0;

                while (cursor < input.Length)
                {
                    // 1. Define Search Window
                    int searchStart = Math.Max(0, cursor - windowSize);
                    int searchLength = cursor - searchStart;

                    // 2. Linear Search for Best Match (No Hash Map)
                    int bestMatchDistance = 0;
                    int bestMatchLength = 0;

                    for (int i = 0; i < searchLength; i++)
                    {
                        int currentMatchLength = 0;

                        // Compare history (searchStart + i) vs Lookahead (cursor)
                        while (currentMatchLength < lookaheadSize &&
                               (cursor + currentMatchLength) < input.Length &&
                               input[searchStart + i + currentMatchLength] == input[cursor + currentMatchLength])
                        {
                            currentMatchLength++;
                        }

                        if (currentMatchLength > bestMatchLength)
                        {
                            bestMatchLength = currentMatchLength;
                            bestMatchDistance = searchLength - i;
                        }
                    }

                    // 3. Write Token to Binary Stream
                    // Structure: [Offset (2 bytes)] [Length (1 byte)] [NextByte (1 byte)]
                    // Total size per token: 4 bytes.

                    if (bestMatchLength > 0 && (cursor + bestMatchLength) < input.Length)
                    {
                        writer.Write((short)bestMatchDistance);
                        writer.Write((byte)bestMatchLength);
                        writer.Write(input[cursor + bestMatchLength]); // The byte AFTER the match

                        cursor += bestMatchLength + 1;
                    }
                    else
                    {
                        // No valid match found (Literal)
                        writer.Write((short)0);
                        writer.Write((byte)0);
                        writer.Write(input[cursor]);

                        cursor++;
                    }
                    tokenCount++;
                }

                return new Lz77Result
                {
                    CompressedData = outputStream.ToArray(),
                    TokenCount = tokenCount
                };
            }
        }
    }
}