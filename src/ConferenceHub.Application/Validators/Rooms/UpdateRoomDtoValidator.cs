using ConferenceHub.Application.DTOs.Rooms;
using FluentValidation;

namespace ConferenceHub.Application.Validators.Rooms;

public class UpdateRoomDtoValidator : AbstractValidator<UpdateRoomDto>
{
    public UpdateRoomDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Room name must be 200 characters or fewer.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0.")
            .LessThanOrEqualTo(1000).WithMessage("Capacity must be 1000 or fewer.");

        RuleFor(x => x.PricePerHour)
            .GreaterThan(0).WithMessage("Price per hour must be greater than 0.");
    }
}
