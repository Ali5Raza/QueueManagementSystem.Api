using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Data;
using QueueManagement.Api.Models;
using QueueManagement.Api.Models.DTOs;
using QueueManagement.Api.Utilities;

namespace QueueManagement.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly ApplicationDbContext _context;

        public TokenService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TokenResponseDto> GenerateTokenAsync(string cnic)
        {
            if (!await ValidateCnicAsync(cnic))
                throw new ArgumentException("Invalid CNIC format");

            // Check if same CNIC has active token
            var encryptedCnic = EncryptionHelper.Encrypt(cnic);
            var existingToken = await _context.Tokens
                .Where(t => t.EncryptedCnic == encryptedCnic && t.Status == 0) // Waiting status
                .FirstOrDefaultAsync();

            if (existingToken != null)
                throw new InvalidOperationException("A token already exists for this CNIC");

            var tokenNumber = await GenerateUniqueTokenNumber();
            var lastFourCnic = cnic.Substring(cnic.Length - 4);

            var token = new Token
            {
                TokenNumber = tokenNumber,
                EncryptedCnic = encryptedCnic,
                LastFourCnic = lastFourCnic,
                CreatedAt = DateTime.UtcNow,
                Status = 0, // Waiting
                CounterId = null, // Will be assigned when called
                QueueId = 1 // Default to first queue
            };

            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();

            return new TokenResponseDto
            {
                Id = token.Id,
                TokenNumber = token.TokenNumber,
                LastFourCnic = token.LastFourCnic,
                CreatedAt = token.CreatedAt,
                Status = token.Status,
                StatusText = GetStatusText(token.Status)
            };
        }

        public async Task<TokenResponseDto?> GetTokenAsync(int tokenId)
        {
            var token = await _context.Tokens.FindAsync(tokenId);
            if (token == null) return null;

            return new TokenResponseDto
            {
                Id = token.Id,
                TokenNumber = token.TokenNumber,
                LastFourCnic = token.LastFourCnic,
                CreatedAt = token.CreatedAt,
                CalledAt = token.CalledAt,
                CompletedAt = token.CompletedAt,
                Status = token.Status,
                StatusText = GetStatusText(token.Status)
            };
        }

        public async Task<bool> ValidateCnicAsync(string cnic)
        {
            return CNICValidator.IsValid(cnic);
        }

        private async Task<string> GenerateUniqueTokenNumber()
        {
            string tokenNumber;
            do
            {
                var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
                var randomPart = new Random().Next(1000, 9999);
                tokenNumber = $"TKN{datePart}{randomPart}";
            }
            while (await _context.Tokens.AnyAsync(t => t.TokenNumber == tokenNumber));

            return tokenNumber;
        }

        private string GetStatusText(int status)
        {
            return status switch
            {
                0 => "Waiting",
                1 => "Called",
                2 => "Completed",
                _ => "Unknown"
            };
        }
    }
}