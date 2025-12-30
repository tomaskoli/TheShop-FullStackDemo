using Catalog.Application.Dtos;
using Catalog.Domain.Entities;
using FluentResults;
using MediatR;
using TheShop.SharedKernel;

namespace Catalog.Application.Queries;

public class GetBrandsQueryHandler : IRequestHandler<GetBrandsQuery, Result<IReadOnlyList<BrandDto>>>
{
    private readonly IRepository<Brand> _brandRepository;

    public GetBrandsQueryHandler(IRepository<Brand> brandRepository)
    {
        _brandRepository = brandRepository;
    }

    public async Task<Result<IReadOnlyList<BrandDto>>> Handle(GetBrandsQuery request, CancellationToken ct)
    {
        var brands = await _brandRepository.GetAllAsync(ct);
        var dtos = brands.Select(b => new BrandDto(b.Id, b.Name)).ToList();
        return Result.Ok<IReadOnlyList<BrandDto>>(dtos);
    }
}

