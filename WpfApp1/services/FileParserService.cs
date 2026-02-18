using Search.Domain.Dto;

namespace Search.services
{
    public class FileParserService
    {
        public UserParsedInput FIleParser(string input)
        {
            var userInputType = input
                .Split(";", StringSplitOptions.RemoveEmptyEntries);

            var fileExtensions = userInputType
                .Where(s => s.Trim().StartsWith("."))
                .SelectMany(s => s.ToLower().Split(",", StringSplitOptions.RemoveEmptyEntries))
                .Select(s => s.Trim())
                .ToHashSet();

            var fileName = userInputType
                .Where(s => !s.Trim().StartsWith("."))
                .SelectMany(s => s.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                .ToList();

            UserParsedInput userParsedInput = new UserParsedInput();

            userParsedInput.Extensions = fileExtensions;
            userParsedInput.QueryText = fileName;

            return userParsedInput;
        }
    }
}
