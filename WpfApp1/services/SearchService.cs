using System.IO;

namespace WpfApp1.servives
{
    public class SearchService
    {
        public async Task<List<FileInfo>> SearchFiles()
        {
            List<Task<List<FileInfo>>> tasks = new();
            var drivers = DriveInfo.GetDrives();
            foreach (var drive in drivers)
            {
                tasks.Add(Task.Run(() => RunSearch(drive.RootDirectory.ToString())));
            }

            var result = await Task.WhenAll(tasks);
            var merged = result.SelectMany(x => x).ToList();

            return merged;
        }
        private List<FileInfo> RunSearch(string startingDirectory)
        {

            var stack = new Stack<DirectoryInfo>();
            stack.Push(new DirectoryInfo(startingDirectory));
            var local = new List<FileInfo>();
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                try
                {
                    foreach (var file in dir.EnumerateFiles("*.*"))
                    {
                        local.Add(file);
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
            return local;
        }
    }
}
