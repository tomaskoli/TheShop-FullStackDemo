using FluentResults;
using MediatR;

namespace Catalog.Application.Commands;

public record DeleteProductCommand(Guid ProductId) : IRequest<Result>;

