using System.ComponentModel.DataAnnotations;
using ConferenceHub.Application.DTOs.Rooms;

namespace ConferenceHub.Web.ViewModels;

public class RoomsIndexViewModel : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Capacity must be greater than 0")]
    [Display(Name = "Min capacity")]
    public int? MinCapacity { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "From")]
    public DateTime? StartTime { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "To")]
    public DateTime? EndTime { get; set; }

    public IReadOnlyList<RoomDto> Rooms { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartTime.HasValue != EndTime.HasValue)
        {
            yield return new ValidationResult(
            "Both From and To must be provided.",
            [nameof(EndTime)]);
        }

        if (StartTime.HasValue && EndTime.HasValue && EndTime <= StartTime)
        {
            yield return new ValidationResult(
            "To must be later than From.",
            [nameof(EndTime)]);
        }
    }
}
