using Application.Models;

namespace Application.Compression
{
    public class Lz77Result
    {
        public byte[] CompressedData { get; set; } = Array.Empty<byte>();

        public int TokenCount { get; set; }
    }
}