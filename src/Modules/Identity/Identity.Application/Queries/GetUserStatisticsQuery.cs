using FluentResults;
using Identity.Application.Dtos;
using MediatR;

namespace Identity.Application.Queries;

public record GetUserStatisticsQuery(DateTime? FromDate, DateTime? ToDate) : IRequest<Result<UserStatisticsDto>>;

