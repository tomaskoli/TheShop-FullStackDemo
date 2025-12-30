using Catalog.Application.Dtos;
using FluentResults;
using MediatR;

namespace Catalog.Application.Queries;

public record GetBrandsQuery : IRequest<Result<IReadOnlyList<BrandDto>>>;

