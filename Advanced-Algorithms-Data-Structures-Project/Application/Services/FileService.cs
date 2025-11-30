namespace Application.Services
{
    public class FileService : IFileService
    {
        public byte[] ReadBytes(string path)
            {
            if (!File.Exists(path))
                throw new FileNotFoundException("File not found: {$path}");
            return File.ReadAllBytes(path);
            }


        public void WriteBytes(string path, byte[] data) =>
            File.WriteAllBytes(path, data);
    }
}
