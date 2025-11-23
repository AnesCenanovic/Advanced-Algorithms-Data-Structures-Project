using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class FileService : IFileService
    {
        public string ReadFile(string path) => File.ReadAllText(path);

        public void WriteFile(string path, string content) =>
            File.WriteAllText(path, content);
    }
}
