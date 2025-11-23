namespace Application.Services
{
    public interface IFileService
    {
        public string ReadFile(string path);
        public void WriteFile(string path, string content);
    }
}
