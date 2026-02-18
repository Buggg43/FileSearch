using System.IO;

namespace Search.Domain
{
    public class FileCache
    {
        public List<FileInfo> FileIndex { get; set; }
        public DateTime IndexBuildAt { get; set; }
    }
}
