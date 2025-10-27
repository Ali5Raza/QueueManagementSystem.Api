namespace QueueManagement.Api.Models.DTOs
{
    public class QueueStatusDto
    {
        public List<TokenResponseDto> WaitingTokens { get; set; } = new();
        public List<TokenResponseDto> CalledTokens { get; set; } = new();
        public List<CounterDto> Counters { get; set; } = new();
    }

    public class CounterDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? CurrentTokenId { get; set; }
        public string? CurrentTokenNumber { get; set; }
    }
}