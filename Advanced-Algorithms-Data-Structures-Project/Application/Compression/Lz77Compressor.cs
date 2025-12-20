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

        public Lz77Result CompressWithHashVerification(byte[] input, int windowSize, int lookaheadSize)
        {
            using (var outputStream = new MemoryStream())
            using (var writer = new BinaryWriter(outputStream))
            {
                int cursor = 0;
                int tokenCount = 0;
                const int minMatchLength = 3; // Minimum match length to consider

                // Hash table: maps 3-byte hash -> list of positions
                var hashTable = new Dictionary<int, List<int>>();

                while (cursor < input.Length)
                {
                    // 1. Define Search Window
                    int searchStart = Math.Max(0, cursor - windowSize);

                    // 2. Hash-based Search for Best Match
                    int bestMatchDistance = 0;
                    int bestMatchLength = 0;

                    // Calculate hash of current lookahead (first 3 bytes)
                    if (cursor + minMatchLength <= input.Length)
                    {
                        int hash = ComputeHash(input, cursor, minMatchLength);

                        // Look up positions with matching hash
                        if (hashTable.TryGetValue(hash, out var positions))
                        {
                            // Check each candidate position within the search window
                            foreach (int pos in positions)
                            {
                                // Only consider positions within current search window
                                if (pos < searchStart || pos >= cursor)
                                    continue;

                                // Verify and extend the match
                                int currentMatchLength = 0;
                                while (currentMatchLength < lookaheadSize &&
                                       (cursor + currentMatchLength) < input.Length &&
                                       input[pos + currentMatchLength] == input[cursor + currentMatchLength])
                                {
                                    currentMatchLength++;
                                }

                                if (currentMatchLength > bestMatchLength)
                                {
                                    bestMatchLength = currentMatchLength;
                                    bestMatchDistance = cursor - pos;
                                }
                            }
                        }

                        // Add current position to hash table for future matches
                        if (!hashTable.ContainsKey(hash))
                        {
                            hashTable[hash] = new List<int>();
                        }
                        hashTable[hash].Add(cursor);

                        // Clean up old entries outside the window
                        if (hashTable[hash].Count > 100) // Limit chain length
                        {
                            hashTable[hash].RemoveAll(p => p < searchStart);
                        }
                    }

                    // 3. Write Token to Binary Stream
                    // Structure: [Offset (2 bytes)] [Length (1 byte)] [NextByte (1 byte)]

                    if (bestMatchLength >= minMatchLength && (cursor + bestMatchLength) < input.Length)
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

        private int ComputeHash(byte[] data, int position, int length)
        {
            // Simple rolling hash using polynomial hashing
            int hash = 0;
            int prime = 31;

            for (int i = 0; i < length && (position + i) < data.Length; i++)
            {
                hash = hash * prime + data[position + i];
            }

            return hash;
        }
    }
}