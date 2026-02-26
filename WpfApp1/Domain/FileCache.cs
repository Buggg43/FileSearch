namespace Search.Domain
{
    public class FileCache
    {
        public List<IndexedFile> FileIndex { get; set; } = new();
        public DateTime IndexBuildAt { get; set; }
    }
}
