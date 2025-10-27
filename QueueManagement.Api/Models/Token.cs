using System.ComponentModel.DataAnnotations;

namespace QueueManagement.Api.Models
{
    public class Token
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20, ErrorMessage = "Token number cannot exceed 20 characters")]
        public string TokenNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200, ErrorMessage = "Encrypted CNIC cannot exceed 200 characters")]
        public string EncryptedCnic { get; set; } = string.Empty;

        [Required]
        [StringLength(4, ErrorMessage = "Last four CNIC cannot exceed 4 characters")]
        public string LastFourCnic { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? CalledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int? CounterId { get; set; }
        public int QueueId { get; set; } = 1; // Default to first queue
        public int Status { get; set; } // 0: Waiting, 1: Called, 2: Completed

        public virtual Counter? Counter { get; set; }
        public virtual Queue? Queue { get; set; }
    }
}