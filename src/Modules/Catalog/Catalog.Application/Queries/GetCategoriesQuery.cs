using Catalog.Application.Dtos;
using FluentResults;
using MediatR;

namespace Catalog.Application.Queries;

public record GetCategoriesQuery : IRequest<Result<IReadOnlyList<CategoryDto>>>;

