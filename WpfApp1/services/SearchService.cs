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
            var userInputType = userInput
                .Split(";", 2, StringSplitOptions.RemoveEmptyEntries);

            var userFileSearch = userInputType[0]
                .ToString();

            string userSelectedExt = "";
            List<string> selectedExt = new List<string>();
            if (userInputType.Length == 1)
            {
                userSelectedExt = userInputType[1];
                var selectedTypes = FileTypeParser(userSelectedExt);

                selectedExt = selectedTypes
                    .Select(s => s.FileType.ToString().ToLower())
                    .ToList();
            }

            var drivers = DriveInfo.GetDrives();
            foreach (var drive in drivers)
            {
                Task.Run(() => RunSearch(progress, drive.RootDirectory.ToString(), userFileSearch, selectedExt));
            }
            await Task.WhenAll();
            return progress;
        }
        private List<FileTypeChecker> FileTypeParser(string input)
        {
            var splitInput = input
                .ToUpper()
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            List<FileTypeChecker> selectedTypes = new List<FileTypeChecker>();

            foreach (var format in splitInput)
            {
                if (Enum.TryParse<FileTypeEnum>(format, out var fileTypeEnum))
                {
                    selectedTypes.Add(new FileTypeChecker { FileType = fileTypeEnum, IsChecked = true });
                }
            }
            return selectedTypes;
        }
        private IProgress<FileInfo> RunSearch(IProgress<FileInfo> progress,
            string startFolder, string userFileSearch, List<string> selectedExtensions)
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
                        if (selectedExtensions != null)
                        {
                            if (selectedExtensions.Equals(file.Extension) &&
                            file.Name.Contains(userFileSearch, StringComparison.OrdinalIgnoreCase))
                            {
                                progress.Report(file);
                            }
                        }
                        else
                        {
                            if (file.Name.Contains(userFileSearch, StringComparison.OrdinalIgnoreCase))
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
