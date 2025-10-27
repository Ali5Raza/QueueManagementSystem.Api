using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Data;
using QueueManagement.Api.Models.DTOs;
using QueueManagement.Api.Services;

namespace QueueManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService _queueService;
        private readonly ApplicationDbContext _context;

        public QueueController(IQueueService queueService, ApplicationDbContext context)
        {
            _queueService = queueService;
            _context = context;
        }

        [HttpGet("status")]
        public async Task<ActionResult<QueueStatusDto>> GetQueueStatus()
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
                    StatusText = GetStatusText(t.Status) // Use static method
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
                    StatusText = GetStatusText(t.Status) // Use static method
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

            return Ok(new QueueStatusDto
            {
                WaitingTokens = waitingTokens,
                CalledTokens = calledTokens,
                Counters = counters
            });
        }

        [HttpPost("call")]
        public async Task<ActionResult<TokenResponseDto>> CallToken([FromBody] CallTokenRequestDto request)
        {
            try
            {
                var token = await _queueService.CallTokenAsync(request);
                return Ok(token);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("complete/{tokenId}")]
        public async Task<IActionResult> CompleteToken(int tokenId)
        {
            try
            {
                await _queueService.CompleteTokenAsync(tokenId);
                return Ok();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Make this method static to avoid memory leaks
        private static string GetStatusText(int status)
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