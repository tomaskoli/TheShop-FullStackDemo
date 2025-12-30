using Basket.Application.Commands;
using Basket.Application.Dtos;
using Basket.Application.Queries;
using Identity.Application.Services;
using MediatR;

namespace TheShop.Api.Endpoints;

public static class BasketEndpoints
{
    public static void MapBasketEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/basket")
            .WithTags("Basket")
            .RequireAuthorization()
            .RequireRateLimiting("Global");

        group.MapGet("/", GetBasket)
            .Produces<BasketDto>();
        group.MapPost("/items", AddItem)
            .Produces<BasketDto>();
        group.MapPut("/items/{productId:guid}", UpdateItem)
            .Produces<BasketDto>();
        group.MapDelete("/items/{productId:guid}", RemoveItem)
            .Produces<BasketDto>();
        group.MapDelete("/", ClearBasket)
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetBasket(
        ICurrentUser currentUser,
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetBasketQuery(currentUser.Id);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> AddItem(
        AddToBasketRequest request,
        ICurrentUser currentUser,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new AddToBasketCommand(
            currentUser.Id,
            request.ProductId,
            request.Quantity);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> UpdateItem(
        Guid productId,
        UpdateBasketItemRequest request,
        ICurrentUser currentUser,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new UpdateBasketCommand(currentUser.Id, productId, request.Quantity);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> RemoveItem(
        Guid productId,
        ICurrentUser currentUser,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new RemoveFromBasketCommand(currentUser.Id, productId);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> ClearBasket(
        ICurrentUser currentUser,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new ClearBasketCommand(currentUser.Id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }
}
