using Search.Domain.Dto;
using System.IO;
using WpfApp1.Domain;

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
                .Split(";", 2, StringSplitOptions.RemoveEmptyEntries);

            var fileExtensions = userInputType
                .Where(s => s.StartsWith("."))
                .ToString();

            var fileName = userInputType
                .Where(s => !s.StartsWith("."))
                .ToString();

            var splitInput = fileExtensions
                .ToLower()
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            UserParsedInput userParsedInput = new UserParsedInput();

            foreach (var format in splitInput)
            {
                if (Enum.TryParse<FileTypeEnum>(format, out var fileTypeEnum))
                {
                    userParsedInput.Extensions.Add(fileTypeEnum.ToString().Insert(0, "."));
                }
            }
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
                        if (userParsedInput.Extensions.Count > 0)
                        {
                            if (userParsedInput.Extensions.Select(s => "." + userParsedInput.Extensions).Contains(file.Extension) &&
                            file.Name.Contains(userParsedInput.QueryText, StringComparison.OrdinalIgnoreCase))
                            {
                                progress.Report(file);
                            }
                        }
                        else
                        {
                            if (file.Name.Contains(userParsedInput.QueryText, StringComparison.OrdinalIgnoreCase))
                            {
                                progress.Report(file);
                            }
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
