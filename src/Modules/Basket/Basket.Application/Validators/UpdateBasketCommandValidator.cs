using Basket.Application.Commands;
using FluentValidation;

namespace Basket.Application.Validators;

public class UpdateBasketCommandValidator : AbstractValidator<UpdateBasketCommand>
{
    public UpdateBasketCommandValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero")
            .LessThanOrEqualTo(100).WithMessage("Quantity must not exceed 100");
    }
}
