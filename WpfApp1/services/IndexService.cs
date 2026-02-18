using Search.Domain;
using System.IO;
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
        public async Task<FileCache> BuildIndexAsync(IProgress<FileInfo> progress)
        {
            await _searchService.SearchFiles("", progress);
            FileCache fileCashe = new FileCache()
            {
                FileIndex = (List<FileInfo>)progress,
                IndexBuildAt = DateTime.UtcNow
            };
            return fileCashe;
        }
    }
}
