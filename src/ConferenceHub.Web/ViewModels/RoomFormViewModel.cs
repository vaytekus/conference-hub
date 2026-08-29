using System.ComponentModel.DataAnnotations;

namespace ConferenceHub.Web.ViewModels;

public class RoomFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, ErrorMessage = "Name must be at most 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "Capacity must be between 1 and 1000.")]
    public int Capacity { get; set; }

    [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100000.")]
    [DataType(DataType.Currency)]
    public decimal PricePerHour { get; set; } = 1m;

    public bool IsEdit => Id.HasValue;
}
