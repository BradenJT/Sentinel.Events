using FluentValidation;
using Sentinel.Application.DTOs.Requests;

namespace Sentinel.Application.Validators
{
    public class DeviceProvisionValidator : AbstractValidator<DeviceProvisionRequest>
    {
        public DeviceProvisionValidator()
        {
            RuleFor(x => x.DeviceId)
                .NotEmpty().WithMessage("Device ID is required")
                .MaximumLength(100).WithMessage("Device ID cannot exceed 100 characters")
                .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Device ID can only contain alphanumeric characters, hyphens, and underscores");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Device name is required")
                .MaximumLength(200).WithMessage("Device name cannot exceed 200 characters")
                .MinimumLength(3).WithMessage("Device name must be at least 3 characters");

            RuleFor(x => x.Metadata)
                .MaximumLength(2000).WithMessage("Metadata cannot exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Metadata));
        }
    }
}
