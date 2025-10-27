using System.ComponentModel.DataAnnotations;

namespace QueueManagement.Api.Models.DTOs
{
    public class TokenRequestDto
    {
        [Required]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "CNIC must be 13 digits")]
        public string Cnic { get; set; } = string.Empty;
    }
}