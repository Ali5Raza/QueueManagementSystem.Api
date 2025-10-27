using Microsoft.AspNetCore.Mvc;
using QueueManagement.Api.Models.DTOs;
using QueueManagement.Api.Services;

namespace QueueManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokensController : ControllerBase
    {
        private readonly ITokenService _tokenService;

        public TokensController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost]
        public async Task<ActionResult<TokenResponseDto>> GenerateToken([FromBody] TokenRequestDto request)
        {
            try
            {
                var token = await _tokenService.GenerateTokenAsync(request.Cnic);
                return CreatedAtAction(nameof(GetToken), new { id = token.Id }, token);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TokenResponseDto>> GetToken(int id)
        {
            var token = await _tokenService.GetTokenAsync(id);
            if (token == null)
                return NotFound();

            return Ok(token);
        }
    }
}