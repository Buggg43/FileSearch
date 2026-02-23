namespace Search.Domain
{
    public class FileCache
    {
        public List<IndexedFile> FileIndex { get; set; } = new List<IndexedFile>();
        public DateTime IndexBuildAt { get; set; }
    }
}
