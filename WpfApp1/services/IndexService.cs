using Search.Domain;
using System.IO;
using System.Text.Json;
using WpfApp1.servives;

namespace Search.services
{
    public class IndexService
    {
        private readonly SearchService _searchService;
        public IndexService(SearchService searchService)
        {
            _searchService = searchService;
        }
        public async Task<FileCache> BuildIndexAsync()
        {
            var result = await _searchService.SearchFiles();
            List<IndexedFile> indexedFile = new List<IndexedFile>();
            foreach (var s in result)
            {
                indexedFile.Add(new IndexedFile
                {
                    FullPath = s.FullName,
                    Name = s.Name,
                    Extension = s.Extension
                });
            }
            FileCache fileCache = new FileCache()
            {
                FileIndex = indexedFile,
                IndexBuildAt = DateTime.UtcNow
            };
            return fileCache;
        }
        public async Task<FileCache> LoadIndexFromDisk()
        {
            string path = "C:\\Users\\Desktop-KW\\source\\repos\\WpfApp1\\WpfApp1\\FileCashe.json";
            string result = "";
            try
            {
                result = await File.ReadAllTextAsync(path);
            }
            catch (FileNotFoundException)
            {
                return new FileCache
                {
                    FileIndex = new List<IndexedFile>(),
                    IndexBuildAt = DateTime.UtcNow
                };
            }
            if (result != string.Empty)
            {
                return new FileCache
                {
                    FileIndex = new List<IndexedFile>(),
                    IndexBuildAt = DateTime.UtcNow
                };
            }

            var deserializedCache = JsonSerializer.Deserialize<FileCache>(result);

            if (deserializedCache.FileIndex.Count > 0)
            {
                return new FileCache { FileIndex = deserializedCache.FileIndex, IndexBuildAt = deserializedCache.IndexBuildAt };
            }

            return new FileCache
            {
                FileIndex = new List<IndexedFile>(),
                IndexBuildAt = DateTime.UtcNow
            };
        }
        public async Task SaveIndexToDisk(List<FileCache> fileCache)
        {
            string path = "C:\\Users\\Desktop-KW\\source\\repos\\WpfApp1\\WpfApp1\\FileCashe.json";
            string tempPath = path + ".tmp";
            string json = JsonSerializer.Serialize(fileCache);

            await File.WriteAllTextAsync(tempPath, json);
            File.Move(tempPath, path);
        }
    }
}
