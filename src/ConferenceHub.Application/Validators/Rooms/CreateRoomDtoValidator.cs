using ConferenceHub.Application.Common;
using ConferenceHub.Application.DTOs.Rooms;
using FluentValidation;

namespace ConferenceHub.Application.Validators.Rooms;

public class CreateRoomDtoValidator : AbstractValidator<CreateRoomDto>
{
    public CreateRoomDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(RoomValidationConstants.MaxNameLength).WithMessage($"Room name must be {RoomValidationConstants.MaxNameLength} characters or fewer.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0.")
            .LessThanOrEqualTo(RoomValidationConstants.MaxCapacity).WithMessage($"Capacity must be {RoomValidationConstants.MaxCapacity} or fewer.");

        RuleFor(x => x.PricePerHour)
            .GreaterThan(0).WithMessage("Price per hour must be greater than 0.");
    }
}
