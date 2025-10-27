using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueManagement.Api.Data;
using QueueManagement.Api.Models;
using QueueManagement.Api.Services;

namespace QueueManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IQueueService _queueService;

        public CountersController(ApplicationDbContext context, IQueueService queueService)
        {
            _context = context;
            _queueService = queueService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Counter>>> GetCounters()
        {
            var counters = await _context.Counters.Where(c => c.IsActive).ToListAsync();
            return Ok(counters);
        }

        [HttpGet("{id}/call-next")]
        public async Task<ActionResult> CallNextToken(int id)
        {
            try
            {
                var token = await _queueService.CallNextTokenAsync(id);
                return Ok(token);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}