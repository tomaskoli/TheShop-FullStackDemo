using Catalog.Application.Dtos;
using Catalog.Domain.Entities;
using FluentResults;
using MediatR;
using TheShop.SharedKernel;

namespace Catalog.Application.Queries;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryDto>>>
{
    private readonly IRepository<Category> _categoryRepository;

    public GetCategoriesQueryHandler(IRepository<Category> categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<Result<IReadOnlyList<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken ct)
    {
        var categories = await _categoryRepository.GetAllAsync(ct);
        var dtos = categories.Select(c => new CategoryDto(c.Id, c.Name, c.Description)).ToList();
        return Result.Ok<IReadOnlyList<CategoryDto>>(dtos);
    }
}

