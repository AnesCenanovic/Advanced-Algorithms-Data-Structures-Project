using Application.Models;

namespace Application.Compression
{
    public interface ILz77Decompressor
    {
        public string Decompress(IEnumerable<Token> tokens);
    }
}
