using System.ComponentModel.DataAnnotations;
using ConferenceHub.Application.DTOs.Services;

namespace ConferenceHub.Web.ViewModels;

public class BookReservationViewModel : IValidatableObject
{
    public Guid RoomId { get; set; }

    [Required(ErrorMessage = "Start time is required")]
    [DataType(DataType.DateTime)]
    [Display(Name = "From")]
    public DateTime? StartTime { get; set; }

    [Required(ErrorMessage = "End time is required")]
    [DataType(DataType.DateTime)]
    [Display(Name = "To")]
    public DateTime? EndTime { get; set; }

    public List<Guid> SelectedServiceIds { get; set; } = [];

    public string RoomName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public decimal PricePerHour { get; set; }

    public IReadOnlyList<ServiceDto> AvailableServices { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RoomId == Guid.Empty)
        {
            yield return new ValidationResult(
            "Room is not specified.",
            [nameof(RoomId)]);
        }

        if (StartTime.HasValue && EndTime.HasValue && EndTime <= StartTime)
        {
            yield return new ValidationResult(
            "To must be later than From.",
            [nameof(EndTime)]);
        }

        if (StartTime.HasValue && StartTime < DateTime.UtcNow)
        {
            yield return new ValidationResult(
            "Cannot book in the past.",
            [nameof(StartTime)]);
        }

        if (StartTime.HasValue && (StartTime.Value.Minute != 0 || StartTime.Value.Second != 0))
        {
            yield return new ValidationResult(
            "From must be on a whole hour (e.g. 14:00).",
            [nameof(StartTime)]);
        }

        if (EndTime.HasValue && (EndTime.Value.Minute != 0 || EndTime.Value.Second != 0))
        {
            yield return new ValidationResult(
            "To must be on a whole hour (e.g. 15:00).",
            [nameof(EndTime)]);
        }
    }
}
