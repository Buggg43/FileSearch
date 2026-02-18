using Search.Domain.Dto;
using Search.services;
using System.IO;

namespace WpfApp1.servives
{
    public class SearchService
    {
        private readonly FileParserService _fileParse;
        private readonly object _indexLock = new();
        public SearchService(FileParserService fileParser)
        {
            _fileParse = fileParser;
        }
        public async Task SearchFiles(
            string userInput,
            IProgress<FileInfo> progress)
        {
            var selectedInput = _fileParse.FIleParser(userInput);

            var drivers = DriveInfo.GetDrives();
            List<Task> tasks = new List<Task>();
            foreach (var drive in drivers)
            {
                tasks.Add(Task.Run(() => RunSearch(
                    progress, selectedInput, drive.RootDirectory.ToString())));
            }
            await Task.WhenAll(tasks);
        }
        private IProgress<FileInfo> RunSearch(IProgress<FileInfo> progress,
            UserParsedInput userParsedInput, string startFolder = @"C:\")
        {

            var stack = new Stack<DirectoryInfo>();
            stack.Push(new DirectoryInfo(startFolder));
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                try
                {
                    foreach (var file in dir.EnumerateFiles("*.*"))
                    {
                        bool extensionMatch = userParsedInput.Extensions.Count == 0 || userParsedInput.Extensions.Contains(file.Extension.ToLower());

                        bool nameMatch = userParsedInput.QueryText.Count == 0 ? true : userParsedInput.QueryText.All(t => file.Name.Contains(t, StringComparison.OrdinalIgnoreCase));

                        if (extensionMatch && nameMatch)
                            lock (_indexLock)
                            {
                                progress.Report(file);
                            }
                    }
                    foreach (var subDir in dir.EnumerateDirectories())
                    {
                        stack.Push(subDir);
                    }

                }
                catch (UnauthorizedAccessException)
                {

                }
                catch (DirectoryNotFoundException)
                {

                }
            }

            return progress;
        }
    }
}
