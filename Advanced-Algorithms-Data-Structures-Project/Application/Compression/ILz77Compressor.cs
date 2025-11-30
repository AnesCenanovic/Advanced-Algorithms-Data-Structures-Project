namespace Application.Compression
{
    public interface ILz77Compressor
    {
        public Lz77Result Compress(byte[] input, int windowSize, int lookaheadSize);
    }
}