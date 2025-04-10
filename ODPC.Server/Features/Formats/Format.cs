namespace ODPC.Features.Formats
{
    public class Format
    {
        public required string Identifier { get; set; }
        public required string Name { get; set; }
        public required string MimeType { get; set; }
        public string? Extension { get; set; }
    }
}
