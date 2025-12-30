using FluentValidation;
using Ordering.Application.Commands;

namespace Ordering.Application.Validators;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.ShippingStreet)
            .NotEmpty().WithMessage("Street address is required")
            .MaximumLength(200).WithMessage("Street address must not exceed 200 characters");

        RuleFor(x => x.ShippingCity)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.ShippingPostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");

        RuleFor(x => x.ShippingCountry)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100).WithMessage("Country must not exceed 100 characters");
    }
}
