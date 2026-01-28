using System.IO;
using WpfApp1.Domain;

namespace WpfApp1.servives
{
    public class SearchService
    {
        public IProgress<FileInfo> SearchForImages(
            List<FileTypeChecker> selectedTypes,
            IProgress<FileInfo> progress,
            string startFolder)
        {
            var allowedExt = selectedTypes
                .Select(s => "." + s.FileType.ToString().ToLower())
                .ToHashSet();

            var stack = new Stack<DirectoryInfo>();
            stack.Push(new DirectoryInfo(startFolder));
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                try
                {
                    foreach (var file in dir.EnumerateFiles("*.*"))
                    {
                        if (allowedExt.Contains(file.Extension))
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
