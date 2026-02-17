using Search.Domain.Dto;
using System.IO;

namespace WpfApp1.servives
{
    public class SearchService
    {
        public async Task<IProgress<FileInfo>> SearchFiles(
            string userInput,
            IProgress<FileInfo> progress)
        {
            var selectedInput = FIleParser(userInput);

            var drivers = DriveInfo.GetDrives();
            List<Task> tasks = new List<Task>();
            foreach (var drive in drivers)
            {
                tasks.Add(Task.Run(() => RunSearch(
                    progress, drive.RootDirectory.ToString(), selectedInput)));
            }
            await Task.WhenAll(tasks);
            return progress;
        }
        private UserParsedInput FIleParser(string input)
        {
            var userInputType = input
                .Split(";", StringSplitOptions.RemoveEmptyEntries);

            var fileExtensions = userInputType
                .Where(s => s.Trim().StartsWith("."))
                .SelectMany(s => s.ToLower().Split(",", StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim())
                .ToHashSet();

            var fileName = userInputType
                .Where(s => !s.Trim().StartsWith("."))
                .SelectMany(s => s.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                .ToList();

            UserParsedInput userParsedInput = new UserParsedInput();

            userParsedInput.Extensions = fileExtensions;
            userParsedInput.QueryText = fileName;

            return userParsedInput;
        }
        private IProgress<FileInfo> RunSearch(IProgress<FileInfo> progress,
            string startFolder, UserParsedInput userParsedInput)
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
                            progress.Report(file);
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
