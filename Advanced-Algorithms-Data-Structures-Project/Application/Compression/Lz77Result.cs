using Application.Models;

namespace Application.Compression
{
    public class Lz77Result
    {
        public List<Token> Tokens { get; } = new();

        public string CompressedText =>
            string.Join(" ", Tokens.Select(t => $"({t.Offset},{t.Length},'{t.NextChar}')"));
    }
}
