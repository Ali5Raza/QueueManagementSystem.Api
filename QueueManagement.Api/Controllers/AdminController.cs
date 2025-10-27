using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Data;
using QueueManagement.Api.Models;
using QueueManagement.Api.Models.DTOs;
using QueueManagement.Api.Services;

namespace QueueManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IQueueService _queueService;
        private readonly ITokenService _tokenService;

        public AdminController(ApplicationDbContext context, IQueueService queueService, ITokenService tokenService)
        {
            _context = context;
            _queueService = queueService;
            _tokenService = tokenService;
        }

        #region Counter Management
        [HttpPost("counters")]
        public async Task<ActionResult<Counter>> CreateCounter([FromBody] CounterUpdateDto counterDto)
        {
            if (counterDto == null)
                return BadRequest("Counter data is required");

            if (string.IsNullOrWhiteSpace(counterDto.Name))
                return BadRequest("Counter name is required");

            if (counterDto.Name.Length > 100)
                return BadRequest("Counter name cannot exceed 100 characters");

            if (counterDto.Description?.Length > 500)
                return BadRequest("Counter description cannot exceed 500 characters");

            var counter = new Counter
            {
                Name = counterDto.Name,
                Description = counterDto.Description ?? string.Empty,
                IsActive = counterDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Counters.Add(counter);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCounter), new { id = counter.Id }, counter);
        }

        [HttpPut("counters/{id}")]
        public async Task<IActionResult> UpdateCounter(int id, [FromBody] CounterUpdateDto counterDto)
        {
            if (counterDto == null)
                return BadRequest("Counter data is required");

            if (string.IsNullOrWhiteSpace(counterDto.Name))
                return BadRequest("Counter name is required");

            if (counterDto.Name.Length > 100)
                return BadRequest("Counter name cannot exceed 100 characters");

            if (counterDto.Description?.Length > 500)
                return BadRequest("Counter description cannot exceed 500 characters");

            var existingCounter = await _context.Counters.FindAsync(id);
            if (existingCounter == null)
                return NotFound($"Counter with ID {id} not found");

            existingCounter.Name = counterDto.Name;
            existingCounter.Description = counterDto.Description ?? string.Empty;
            existingCounter.IsActive = counterDto.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating counter: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the counter");
            }
        }

        [HttpDelete("counters/{id}")]
        public async Task<IActionResult> DeleteCounter(int id)
        {
            var counter = await _context.Counters.FindAsync(id);
            if (counter == null)
                return NotFound();

            // Deactivate instead of delete to maintain referential integrity
            counter.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("counters/{id}")]
        public async Task<ActionResult<Counter>> GetCounter(int id)
        {
            var counter = await _context.Counters.FindAsync(id);
            if (counter == null)
                return NotFound();

            return Ok(counter);
        }

        [HttpGet("counters")]
        public async Task<ActionResult<IEnumerable<Counter>>> GetCounters()
        {
            var counters = await _context.Counters.ToListAsync();
            return Ok(counters);
        }
        #endregion

        #region Queue Management
        [HttpPost("queues")]
        public async Task<ActionResult<Queue>> CreateQueue([FromBody] QueueUpdateDto queueDto)
        {
            if (queueDto == null)
                return BadRequest("Queue data is required");

            if (string.IsNullOrWhiteSpace(queueDto.Name))
                return BadRequest("Queue name is required");

            if (queueDto.Name.Length > 100)
                return BadRequest("Queue name cannot exceed 100 characters");

            if (queueDto.Description?.Length > 500)
                return BadRequest("Queue description cannot exceed 500 characters");

            var queue = new Queue
            {
                Name = queueDto.Name,
                Description = queueDto.Description ?? string.Empty,
                IsActive = queueDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Queues.Add(queue);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetQueue), new { id = queue.Id }, queue);
        }

        [HttpPut("queues/{id}")]
        public async Task<IActionResult> UpdateQueue(int id, [FromBody] QueueUpdateDto queueDto)
        {
            if (queueDto == null)
                return BadRequest("Queue data is required");

            if (string.IsNullOrWhiteSpace(queueDto.Name))
                return BadRequest("Queue name is required");

            if (queueDto.Name.Length > 100)
                return BadRequest("Queue name cannot exceed 100 characters");

            if (queueDto.Description?.Length > 500)
                return BadRequest("Queue description cannot exceed 500 characters");

            var existingQueue = await _context.Queues.FindAsync(id);
            if (existingQueue == null)
                return NotFound($"Queue with ID {id} not found");

            existingQueue.Name = queueDto.Name;
            existingQueue.Description = queueDto.Description ?? string.Empty;
            existingQueue.IsActive = queueDto.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating queue: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the queue");
            }
        }

        [HttpDelete("queues/{id}")]
        public async Task<IActionResult> DeleteQueue(int id)
        {
            var queue = await _context.Queues.FindAsync(id);
            if (queue == null)
                return NotFound();

            // Remove all tokens associated with this queue first
            var tokens = await _context.Tokens
                .Where(t => t.QueueId == id)
                .ToListAsync();

            _context.Tokens.RemoveRange(tokens);
            _context.Queues.Remove(queue);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("queues/{id}")]
        public async Task<ActionResult<Queue>> GetQueue(int id)
        {
            var queue = await _context.Queues.FindAsync(id);
            if (queue == null)
                return NotFound();

            return Ok(queue);
        }

        [HttpGet("queues")]
        public async Task<ActionResult<IEnumerable<Queue>>> GetQueues()
        {
            var queues = await _context.Queues.ToListAsync();
            return Ok(queues);
        }
        #endregion

        #region Token Management
        [HttpGet("tokens")]
        public async Task<ActionResult<IEnumerable<TokenResponseDto>>> GetTokens()
        {
            var tokens = await _context.Tokens
                .Select(t => new TokenResponseDto
                {
                    Id = t.Id,
                    TokenNumber = t.TokenNumber,
                    LastFourCnic = t.LastFourCnic,
                    CreatedAt = t.CreatedAt,
                    CalledAt = t.CalledAt,
                    CompletedAt = t.CompletedAt,
                    Status = t.Status,
                    StatusText = GetStatusText(t.Status) // Use static method
                })
                .ToListAsync();

            return Ok(tokens);
        }

        [HttpGet("tokens/{id}")]
        public async Task<ActionResult<TokenResponseDto>> GetToken(int id)
        {
            var token = await _context.Tokens.FindAsync(id);
            if (token == null)
                return NotFound();

            var response = new TokenResponseDto
            {
                Id = token.Id,
                TokenNumber = token.TokenNumber,
                LastFourCnic = token.LastFourCnic,
                CreatedAt = token.CreatedAt,
                CalledAt = token.CalledAt,
                CompletedAt = token.CompletedAt,
                Status = token.Status,
                StatusText = GetStatusText(token.Status) // Use static method
            };

            return Ok(response);
        }

        [HttpPut("tokens/{id}/status")]
        public async Task<IActionResult> UpdateTokenStatus(int id, [FromBody] int status)
        {
            var token = await _context.Tokens.FindAsync(id);
            if (token == null)
                return NotFound();

            if (status < 0 || status > 2)
                return BadRequest("Invalid status value. Use 0=Waiting, 1=Called, 2=Completed");

            token.Status = status;

            if (status == 1 && !token.CalledAt.HasValue)
                token.CalledAt = DateTime.UtcNow;
            else if (status == 2 && !token.CompletedAt.HasValue)
                token.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("tokens/{id}")]
        public async Task<IActionResult> DeleteToken(int id)
        {
            var token = await _context.Tokens.FindAsync(id);
            if (token == null)
                return NotFound();

            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region Queue Status and Statistics
        [HttpGet("dashboard/stats")]
        public async Task<ActionResult<object>> GetDashboardStats()
        {
            var totalTokens = await _context.Tokens.CountAsync();
            var waitingTokens = await _context.Tokens.CountAsync(t => t.Status == 0);
            var calledTokens = await _context.Tokens.CountAsync(t => t.Status == 1);
            var completedTokens = await _context.Tokens.CountAsync(t => t.Status == 2);
            var activeCounters = await _context.Counters.CountAsync(c => c.IsActive);

            var stats = new
            {
                TotalTokens = totalTokens,
                WaitingTokens = waitingTokens,
                CalledTokens = calledTokens,
                CompletedTokens = completedTokens,
                ActiveCounters = activeCounters,
                AverageWaitTime = await CalculateAverageWaitTime(),
                TodayTokens = await GetTodaysTokenCount()
            };

            return Ok(stats);
        }

        [HttpGet("tokens/history")]
        public async Task<ActionResult<IEnumerable<TokenResponseDto>>> GetTokenHistory(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var query = _context.Tokens.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(t => t.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(t => t.CreatedAt <= endDate.Value);

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            var tokens = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TokenResponseDto
                {
                    Id = t.Id,
                    TokenNumber = t.TokenNumber,
                    LastFourCnic = t.LastFourCnic,
                    CreatedAt = t.CreatedAt,
                    CalledAt = t.CalledAt,
                    CompletedAt = t.CompletedAt,
                    Status = t.Status,
                    StatusText = GetStatusText(t.Status) // Use static method
                })
                .ToListAsync();

            return Ok(tokens);
        }
        #endregion

        #region Helper Methods
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

        private async Task<double> CalculateAverageWaitTime()
        {
            var completedTokens = await _context.Tokens
                .Where(t => t.Status == 2 && t.CalledAt.HasValue && t.CreatedAt < t.CalledAt.Value)
                .ToListAsync();

            if (!completedTokens.Any())
                return 0;

            var totalWaitTime = completedTokens
                .Select(t => (t.CalledAt.Value - t.CreatedAt).TotalMinutes)
                .Average();

            return Math.Round(totalWaitTime, 2);
        }

        private async Task<int> GetTodaysTokenCount()
        {
            var today = DateTime.Today;
            return await _context.Tokens.CountAsync(t => t.CreatedAt.Date == today);
        }
        #endregion
    }
}