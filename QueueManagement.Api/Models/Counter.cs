using System.ComponentModel.DataAnnotations;

namespace QueueManagement.Api.Models
{
    public class Counter
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CurrentTokenId { get; set; }

        public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
    }
}