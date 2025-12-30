using FluentResults;
using MediatR;
using Ordering.Application.Dtos;

namespace Ordering.Application.Queries;

public record GetSalesStatisticsQuery(
    DateTime? FromDate,
    DateTime? ToDate,
    Guid? CategoryId,
    Guid? BrandId) : IRequest<Result<SalesStatisticsDto>>;

