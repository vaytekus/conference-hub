using ConferenceHub.Application.DTOs.Reports;
using FluentValidation;

namespace ConferenceHub.Application.Validators.Reports;

public class PeriodQueryDtoValidator : AbstractValidator<PeriodQueryDto>
{
    public PeriodQueryDtoValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Start date must be earlier than or equal to end date.");

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("End date cannot be in the future.");
    }
}
