using Catalog.Application.Commands;
using Catalog.Application.Dtos;
using Catalog.Application.Queries;
using MediatR;
using TheShop.Api.Auth;
using TheShop.SharedKernel;

namespace TheShop.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalog")
            .WithTags("Catalog")
            .RequireRateLimiting("Global");

        group.MapGet("/products", GetProducts)
            .AllowAnonymous()
            .Produces<PagedResult<ProductDto>>();
        group.MapGet("/products/{id:guid}", GetProduct)
            .AllowAnonymous()
            .Produces<ProductDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/products", CreateProduct)
            .RequireAuthorization(Policies.AdminOnly)
            .Produces<ProductDto>(StatusCodes.Status201Created);
        group.MapPut("/products/{id:guid}", UpdateProduct)
            .RequireAuthorization(Policies.AdminOnly)
            .Produces<ProductDto>();
        group.MapDelete("/products/{id:guid}", DeleteProduct)
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapGet("/brands", GetBrands)
            .AllowAnonymous()
            .Produces<IReadOnlyList<BrandDto>>();
        group.MapGet("/categories", GetCategories)
            .AllowAnonymous()
            .Produces<IReadOnlyList<CategoryDto>>();
    }

    private static async Task<IResult> GetProducts(
        int page,
        int pageSize,
        string? search,
        Guid? brandId,
        Guid? categoryId,
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetProductsQuery(
            page > 0 ? page : 1,
            pageSize > 0 ? pageSize : 10,
            search,
            brandId,
            categoryId);

        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> GetProduct(
        Guid id,
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetProductQuery(id);
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> CreateProduct(
        CreateProductRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.BrandId,
            request.CategoryId);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Created($"/api/catalog/products/{result.Value.Id}", result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> UpdateProduct(
        Guid id,
        UpdateProductRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id,
            request.Name,
            request.Description,
            request.Price,
            request.ImageUrl,
            request.BrandId,
            request.CategoryId);

        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> DeleteProduct(
        Guid id,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new DeleteProductCommand(id);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.NotFound(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> GetBrands(
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetBrandsQuery();
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static async Task<IResult> GetCategories(
        ISender mediator,
        CancellationToken ct)
    {
        var query = new GetCategoriesQuery();
        var result = await mediator.Send(query, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }
}

