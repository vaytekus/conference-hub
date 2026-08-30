using ConferenceHub.Application.Common;
using ConferenceHub.Application.DTOs.Reservations;
using FluentValidation;

namespace ConferenceHub.Application.Validators.Reservations;

public class CreateReservationDtoValidator : AbstractValidator<CreateReservationDto>
{
    public CreateReservationDtoValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId is required");

        RuleFor(x => x.StartTime)
            .Must(BeInFuture).WithMessage("StartTime must be in the future.")
            .Must(BookingConstants.IsWholeHour).WithMessage("StartTime must be a whole hour (no minutes/seconds).")
            .Must(BeWithinOperatingHours).WithMessage($"StartTime must be between {BookingConstants.OpeningHour:00}:00 and {BookingConstants.ClosingHour - 1:00}:00.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime.")
            .Must(BookingConstants.IsWholeHour).WithMessage("EndTime must be a whole hour (no minutes/seconds).")
            .Must(BeEndWithinOperatingHours).WithMessage($"EndTime must be between {BookingConstants.OpeningHour + 1:00}:00 and {BookingConstants.ClosingHour:00}:00.");

        RuleFor(x => x.ServiceIds)
            .NotNull().WithMessage("ServiceIds must not be null (use empty list if no services).");

        RuleForEach(x => x.ServiceIds)
            .NotEmpty().WithMessage("ServiceIds must not be empty.");

        RuleFor(x => x.ServiceIds)
            .Must(HaveUniqueIds).WithMessage("ServiceIds must have unique ids.")
            .When(x => x.ServiceIds is not null);
    }

    private static bool BeInFuture(DateTime time)
        => time > DateTime.UtcNow;

    private static bool BeWithinOperatingHours(DateTime startTime)
        => startTime.Hour >= BookingConstants.OpeningHour && startTime.Hour < BookingConstants.ClosingHour;

    private static bool BeEndWithinOperatingHours(DateTime endTime)
        => endTime.Hour > BookingConstants.OpeningHour && endTime.Hour <= BookingConstants.ClosingHour;

    private static bool HaveUniqueIds(IReadOnlyList<Guid> ids)
        => ids.Distinct().Count() == ids.Count;
}
