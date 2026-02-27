using Search.Domain;

namespace Search.services
{
    public class FilteringService
    {
        private readonly FileCache _cache;
        private readonly FileParserService _fileParserService;
        public FilteringService(FileCache cache, FileParserService fileParserService)
        {
            _cache = cache;
            _fileParserService = fileParserService;
        }
        public async Task<List<IndexedFile>> FilterResults(string searchQuery)
        {
            var userSearchQuery = _fileParserService.FIleParser(searchQuery);

            var queryTexts = userSearchQuery.QueryText ?? new List<string>();
            var extensions = userSearchQuery.Extensions;

            var result = _cache.FileIndex
                .Where(file =>
                    (queryTexts.Count == 0 || queryTexts.All(q => file.Name.Contains(q, StringComparison.OrdinalIgnoreCase))) &&
                    (extensions == null || extensions.Count == 0 || extensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                )
                .ToList();

            await Task.Yield();

            return result;
        }
    }
}
