using QueueManagement.Api.Models.DTOs;

namespace QueueManagement.Api.Services
{
    public interface IQueueService
    {
        Task<TokenResponseDto> CallNextTokenAsync(int counterId);
        Task<TokenResponseDto> CallTokenAsync(CallTokenRequestDto request);
        Task<QueueStatusDto> GetQueueStatusAsync();
        Task CompleteTokenAsync(int tokenId);
    }
}