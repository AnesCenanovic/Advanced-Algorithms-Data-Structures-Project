namespace Application.Compression
{
    public interface ILz77Decompressor
    {
        public byte[] Decompress(byte[] input);
    }
}
