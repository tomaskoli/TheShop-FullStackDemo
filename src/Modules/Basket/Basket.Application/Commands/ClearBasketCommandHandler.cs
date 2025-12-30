using Basket.Application.Services;
using FluentResults;
using MediatR;

namespace Basket.Application.Commands;

public class ClearBasketCommandHandler : IRequestHandler<ClearBasketCommand, Result>
{
    private readonly IBasketService _basketService;

    public ClearBasketCommandHandler(IBasketService basketService)
    {
        _basketService = basketService;
    }

    public async Task<Result> Handle(ClearBasketCommand request, CancellationToken ct)
    {
        await _basketService.DeleteBasketAsync(request.BuyerId, ct);
        return Result.Ok();
    }
}

