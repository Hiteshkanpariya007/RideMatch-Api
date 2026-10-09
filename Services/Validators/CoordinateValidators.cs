using Services.Models;
using FluentValidation;

namespace Services.Validators
{
    public sealed class LocationRequestValidator : AbstractValidator<LocationRequest>
    {
        public LocationRequestValidator()
        {
            RuleFor(request => request.Latitude).InclusiveBetween(-90, 90);
            RuleFor(request => request.Longitude).InclusiveBetween(-180, 180);
        }
    }

    public sealed class CreateRideRequestValidator : AbstractValidator<CreateRideRequest>
    {
        public CreateRideRequestValidator()
        {
            RuleFor(request => request.PickupLatitude).InclusiveBetween(-90, 90);
            RuleFor(request => request.PickupLongitude).InclusiveBetween(-180, 180);
            RuleFor(request => request.DestinationLatitude).InclusiveBetween(-90, 90);
            RuleFor(request => request.DestinationLongitude).InclusiveBetween(-180, 180);
        }
    }

    public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
            RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(320);
            RuleFor(request => request.Password).NotEmpty().MinimumLength(8);
            RuleFor(request => request.PhoneNumber).NotEmpty().MaximumLength(30);
            When(request => request.Role == Common.Enums.UserRole.Driver, () =>
            {
                RuleFor(request => request.VehicleType).NotEmpty().MaximumLength(50);
                RuleFor(request => request.VehicleNumber).NotEmpty().MaximumLength(30);
            });
        }
    }

    public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(request => request.Email).NotEmpty().EmailAddress();
            RuleFor(request => request.Password).NotEmpty();
        }
    }
}
