namespace QueueManagement.Api.Models.DTOs
{
    public class TokenResponseDto
    {
        public int Id { get; set; }
        public string TokenNumber { get; set; } = string.Empty;
        public string LastFourCnic { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? CalledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
    }
}