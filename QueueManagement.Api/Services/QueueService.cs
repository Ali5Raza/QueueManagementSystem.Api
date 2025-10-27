using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Data;
using QueueManagement.Api.Hubs;
using QueueManagement.Api.Models;
using QueueManagement.Api.Models.DTOs;

namespace QueueManagement.Api.Services
{
    public class QueueService : IQueueService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAudioService _audioService;
        private readonly IHubContext<QueueHub> _hubContext;

        public QueueService(ApplicationDbContext context, IAudioService audioService, IHubContext<QueueHub> hubContext)
        {
            _context = context;
            _audioService = audioService;
            _hubContext = hubContext;
        }

        public async Task<TokenResponseDto> CallNextTokenAsync(int counterId)
        {
            var waitingToken = await _context.Tokens
                .Where(t => t.Status == 0 && t.CounterId == null) // Waiting and not assigned to counter
                .OrderBy(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (waitingToken == null)
                throw new InvalidOperationException("No tokens in queue");

            return await CallTokenAsync(new CallTokenRequestDto
            {
                TokenId = waitingToken.Id,
                CounterId = counterId
            });
        }

        public async Task<TokenResponseDto> CallTokenAsync(CallTokenRequestDto request)
        {
            var token = await _context.Tokens.FindAsync(request.TokenId);
            if (token == null || token.Status != 0) // Must be waiting
                throw new InvalidOperationException("Token not found or not in waiting state");

            var counter = await _context.Counters.FindAsync(request.CounterId);
            if (counter == null || !counter.IsActive)
                throw new InvalidOperationException("Counter not found or inactive");

            // Update token status and assign to counter
            token.Status = 1; // Called
            token.CalledAt = DateTime.UtcNow;
            token.CounterId = request.CounterId;

            await _context.SaveChangesAsync();

            // Update counter's current token
            counter.CurrentTokenId = token.Id;
            await _context.SaveChangesAsync();

            // Generate audio announcement
            var announcement = $"Token {token.TokenNumber} with CNIC ending in {token.LastFourCnic}, please proceed to {counter.Name}";
            await _audioService.SpeakAsync(announcement);

            // Notify all clients
            await _hubContext.Clients.All.SendAsync("TokenCalled", new
            {
                TokenId = token.Id,
                TokenNumber = token.TokenNumber,
                LastFourCnic = token.LastFourCnic,
                CounterName = counter.Name
            });

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

        public async Task<QueueStatusDto> GetQueueStatusAsync()
        {
            var waitingTokens = await _context.Tokens
                .Where(t => t.Status == 0)
                .Select(t => new TokenResponseDto
                {
                    Id = t.Id,
                    TokenNumber = t.TokenNumber,
                    LastFourCnic = t.LastFourCnic,
                    CreatedAt = t.CreatedAt,
                    Status = t.Status,
                    StatusText = GetStatusText(t.Status)
                })
                .ToListAsync();

            var calledTokens = await _context.Tokens
                .Where(t => t.Status == 1)
                .Select(t => new TokenResponseDto
                {
                    Id = t.Id,
                    TokenNumber = t.TokenNumber,
                    LastFourCnic = t.LastFourCnic,
                    CreatedAt = t.CreatedAt,
                    CalledAt = t.CalledAt,
                    Status = t.Status,
                    StatusText = GetStatusText(t.Status)
                })
                .ToListAsync();

            var counters = await _context.Counters
                .Where(c => c.IsActive)
                .Select(c => new CounterDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CurrentTokenId = c.CurrentTokenId != 0 ? (int?)c.CurrentTokenId : null,
                    CurrentTokenNumber = c.CurrentTokenId != 0 ?
                        _context.Tokens.Where(t => t.Id == c.CurrentTokenId).Select(t => t.TokenNumber).FirstOrDefault() : null
                })
                .ToListAsync();

            return new QueueStatusDto
            {
                WaitingTokens = waitingTokens,
                CalledTokens = calledTokens,
                Counters = counters
            };
        }

        public async Task CompleteTokenAsync(int tokenId)
        {
            var token = await _context.Tokens.FindAsync(tokenId);
            if (token == null || token.Status != 1) // Must be called
                throw new InvalidOperationException("Token not found or not in called state");

            token.Status = 2; // Completed
            token.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Update counter's current token to null
            if (token.CounterId.HasValue)
            {
                var counter = await _context.Counters.FindAsync(token.CounterId.Value);
                if (counter != null)
                {
                    counter.CurrentTokenId = null;
                    await _context.SaveChangesAsync();
                }
            }

            // Notify all clients
            await _hubContext.Clients.All.SendAsync("TokenCompleted", new { TokenId = tokenId });
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