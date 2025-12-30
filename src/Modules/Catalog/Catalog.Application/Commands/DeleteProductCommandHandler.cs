using Catalog.Domain.Entities;
using FluentResults;
using MediatR;
using TheShop.SharedKernel;

namespace Catalog.Application.Commands;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Result>
{
    private readonly IRepository<Product> _productRepository;

    public DeleteProductCommandHandler(IRepository<Product> productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken ct)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, ct);

        if (product is null)
        {
            return Result.Fail("Product not found");
        }

        await _productRepository.DeleteAsync(product, ct);
        await _productRepository.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

