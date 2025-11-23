namespace Application.Compression
{
    public interface ILz77Compressor
    {
        public Lz77Result Compress(string input, int windowSize, int lookaheadSize);
    }
}
