using Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Compression
{
    public class Lz77Result
    {
        public List<Token> Tokens { get; } = new();

        public string CompressedText =>
            string.Join(" ", Tokens.Select(t => $"({t.Offset},{t.Length},'{t.NextChar}')"));
    }
}
