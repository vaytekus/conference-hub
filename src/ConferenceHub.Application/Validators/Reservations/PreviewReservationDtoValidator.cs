using ConferenceHub.Application.DTOs.Reservations;
using FluentValidation;

namespace ConferenceHub.Application.Validators.Reservations;

public class PreviewReservationDtoValidator : AbstractValidator<PreviewReservationDto>
{
    private const int OpeningHour = 6;
    private const int ClosingHour = 23;

    public PreviewReservationDtoValidator()
    {
        RuleFor(x => x.RoomId)
            .NotEmpty().WithMessage("RoomId is required");

        RuleFor(x => x.StartTime)
            .Must(BeWholeHour).WithMessage("StartTime must be a whole hour (no minutes/seconds).")
            .Must(BeWithinOperatingHours).WithMessage($"StartTime must be between {OpeningHour:00}:00 and {ClosingHour - 1:00}:00.");

        RuleFor(x => x.EndTime)
            .GreaterThan(x => x.StartTime).WithMessage("EndTime must be after StartTime.")
            .Must(BeWholeHour).WithMessage("EndTime must be a whole hour (no minutes/seconds).")
            .Must(BeEndWithinOperatingHours).WithMessage($"EndTime must be between {OpeningHour + 1:00}:00 and {ClosingHour:00}:00.");

        RuleFor(x => x.ServiceIds)
            .NotNull().WithMessage("ServiceIds must not be null (use empty list if no services).");

        RuleForEach(x => x.ServiceIds)
            .NotEmpty().WithMessage("ServiceIds must not be empty.");

        RuleFor(x => x.ServiceIds)
            .Must(HaveUniqueIds).WithMessage("ServiceIds must have unique ids.")
            .When(x => x.ServiceIds is not null);
    }

    private static bool BeWholeHour(DateTime time)
        => time.Minute == 0 && time.Second == 0 && time.Millisecond == 0;

    private static bool BeWithinOperatingHours(DateTime startTime)
        => startTime.Hour >= OpeningHour && startTime.Hour < ClosingHour;

    private static bool BeEndWithinOperatingHours(DateTime endTime)
        => endTime.Hour > OpeningHour && endTime.Hour <= ClosingHour;

    private static bool HaveUniqueIds(IReadOnlyList<Guid> ids)
        => ids.Distinct().Count() == ids.Count;
}
