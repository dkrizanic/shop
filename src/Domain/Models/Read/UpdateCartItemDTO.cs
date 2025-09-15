using System.ComponentModel.DataAnnotations;

namespace Domain.Models.Read;

public class UpdateCartItemDTO
{
    [Required]
    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100")]
    public int Quantity { get; set; }
}