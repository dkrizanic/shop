using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Read
{
    public class UserFavoriteDTO
    {
        public int Id { get; set; }
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public int ProductId { get; set; }
        
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}