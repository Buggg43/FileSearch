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
        public async Task<FileCache?> LoadOrBuildAsync()
        {
            var result = await TryLoadIndexFromDisk();
            if (result.FileIndex.Count > 0)
            {
                return result;
            }
            else
            {
                var newIndex = await BuildIndexAsync();
                await SaveIndexToDisk(newIndex);
                return newIndex;
            }
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
        public async Task<FileCache?> TryLoadIndexFromDisk()
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
                    IndexBuildAt = DateTime.UtcNow
                };
            }
            if (string.IsNullOrEmpty(result))
            {
                return new FileCache
                {
                    IndexBuildAt = DateTime.UtcNow
                };
            }
            FileCache? deserializedCache = null;
            try
            {
                deserializedCache = JsonSerializer.Deserialize<FileCache>(result);
            }
            catch (JsonException)
            {
                return new FileCache
                {
                    IndexBuildAt = DateTime.UtcNow
                };
            }
            catch (DirectoryNotFoundException)
            {
                return new FileCache
                {
                    IndexBuildAt = DateTime.UtcNow
                };
            }
            catch (UnauthorizedAccessException)
            {
                return new FileCache
                {
                    IndexBuildAt = DateTime.UtcNow
                };
            }
            if (deserializedCache == null)
            {
                return new FileCache() { IndexBuildAt = DateTime.UtcNow };
            }
            else
                return deserializedCache;
        }
        public async Task SaveIndexToDisk(FileCache fileCache)
        {
            string path = "C:\\Users\\Desktop-KW\\source\\repos\\WpfApp1\\WpfApp1\\FileCashe.json";
            string tempPath = path + ".tmp";
            string backupPath = "C:\\Users\\Desktop-KW\\source\\repos\\WpfApp1\\WpfApp1\\ReplacedFilesBackup.json";
            string json = JsonSerializer.Serialize(fileCache);

            await File.WriteAllTextAsync(tempPath, json);

            if (File.Exists(path))
                File.Replace(tempPath, path, backupPath);
            else
                File.Move(tempPath, path);
        }
    }
}
