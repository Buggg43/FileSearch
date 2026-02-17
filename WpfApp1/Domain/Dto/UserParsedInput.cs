namespace Search.Domain.Dto
{
    public class UserParsedInput
    {
        public List<string>? QueryText { get; set; }
        public HashSet<string>? Extensions { get; set; }
    }
}
