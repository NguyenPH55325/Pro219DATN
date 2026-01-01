namespace Pro219.API.DTOs
{
    public class EmailDTO
    {
        public string? Name { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Subject { get; set; } = string.Empty;
        public string? Body { get; set; } = string.Empty;
        public string? BodyHTML { get; set; } =  string.Empty;
    }
}
