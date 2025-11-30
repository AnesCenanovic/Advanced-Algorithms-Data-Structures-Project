namespace Application.Services
{
    public interface IFileService
    {
        public byte[] ReadBytes(string path);
        public void WriteBytes(string path, byte[] data);
    }
}
