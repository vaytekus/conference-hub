using System.ComponentModel.DataAnnotations;

namespace ConferenceHub.Web.ViewModels;

public class ServiceFormViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(200, ErrorMessage = "Name must be at most 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, 100000, ErrorMessage =  "Price must be between 0.01 and 100000.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; } = 1m;

    public bool IsEdit => Id.HasValue;
}
