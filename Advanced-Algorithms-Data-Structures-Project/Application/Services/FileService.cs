namespace Application.Services
{
    public class FileService : IFileService
    {
        public string ReadFile(string path) => File.ReadAllText(path);

        public void WriteFile(string path, string content) =>
            File.WriteAllText(path, content);
    }
}
