using System.Text.Json;
using Identity.Application.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Commands;
using Ordering.Application.Dtos;
using Ordering.Application.Queries;
using TheShop.Api.Auth;
using TheShop.SharedKernel;

using PagedResult = TheShop.SharedKernel.PagedResult<Ordering.Application.Dtos.OrderDto>;

namespace TheShop.Api.Endpoints;

public static class OrderingEndpoints
{
    public static void MapOrderingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Ordering")
            .RequireAuthorization()
            .RequireRateLimiting("Global");

        group.MapGet("/", GetOrders)
            .Produces<IReadOnlyList<OrderDto>>();
        group.MapGet("/{id:guid}", GetOrder)
            .Produces<OrderDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
        group.MapPost("/", CreateOrder)
            .Produces<OrderDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        group.MapPut("/{id:guid}/cancel", CancelOrder)
            .Produces(StatusCodes.Status204NoContent);
        group.MapPut("/{id:guid}/ship", ShipOrder)
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(StatusCodes.Status204NoContent);
        group.MapGet("/all", GetAllOrders)
            .RequireAuthorization(Policies.AdminOnly)
            .Produces<PagedResult>();
        group.MapGet("/statistics", GetSalesStatistics)
            .RequireAuthorization(Policies.AdminOnly)
            .Produces<SalesStatisticsDto>();
    }

    private static async Task<IResult> GetOrders(
        ICurrentUser currentUser,
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetUserOrdersQuery(currentUser.Id);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> GetOrder(
        Guid id,
        ICurrentUser currentUser,
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetOrderQuery(id);
        var result = await mediator.Send(query, ct);

        if (result.IsFailed)
        {
            return Results.NotFound(result.Errors.Select(e => e.Message));
        }

        if (result.Value.BuyerId != currentUser.Id && !currentUser.IsAdmin)
        {
            return Results.Forbid();
        }

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateOrder(
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CreateOrderRequest request,
        ICurrentUser currentUser,
        ISender mediator,
        IIdempotencyService idempotencyService,
        CancellationToken ct)
    {
        var fullIdempotencyKey = idempotencyKey != null 
            ? $"order:{currentUser.Id}:{idempotencyKey}" 
            : null;

        if (fullIdempotencyKey != null)
        {
            var cached = await idempotencyService.GetAsync(fullIdempotencyKey, ct);
            if (cached != null)
            {
                return Results.Json(
                    JsonSerializer.Deserialize<object>(cached.ResponseBody),
                    statusCode: cached.StatusCode);
            }
        }

        var command = new CreateOrderCommand(
            currentUser.Id,
            request.ShippingStreet,
            request.ShippingCity,
            request.ShippingPostalCode,
            request.ShippingCountry);

        var result = await mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            var responseBody = JsonSerializer.Serialize(result.Value);
            
            if (fullIdempotencyKey != null)
            {
                await idempotencyService.StoreAsync(fullIdempotencyKey, 201, responseBody, ct: ct);
            }
            
            return Results.Created($"/api/orders/{result.Value.Id}", result.Value);
        }

        var errorResponse = result.Errors.Select(e => e.Message).ToList();
        return Results.BadRequest(errorResponse);
    }

    private static async Task<IResult> CancelOrder(
        Guid id,
        ICurrentUser currentUser,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new CancelOrderCommand(id, currentUser.Id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> ShipOrder(
        Guid id,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new ShipOrderCommand(id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> GetAllOrders(
        int page,
        int pageSize,
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetAllOrdersQuery(
            page > 0 ? page : 1,
            pageSize > 0 ? pageSize : 10);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> GetSalesStatistics(
        DateTime? fromDate,
        DateTime? toDate,
        Guid? categoryId,
        Guid? brandId,
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetSalesStatisticsQuery(fromDate, toDate, categoryId, brandId);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }
}
