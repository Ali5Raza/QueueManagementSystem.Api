using QueueManagement.Api.Models.DTOs;

namespace QueueManagement.Api.Services
{
    public interface ITokenService
    {
        Task<TokenResponseDto> GenerateTokenAsync(string cnic);
        Task<TokenResponseDto?> GetTokenAsync(int tokenId);
        Task<bool> ValidateCnicAsync(string cnic);
    }
}