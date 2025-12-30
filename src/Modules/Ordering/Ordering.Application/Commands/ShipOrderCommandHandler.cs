using FluentResults;
using MediatR;
using Ordering.Domain.Aggregates;
using Ordering.Domain.Enums;
using Ordering.Domain.Events;
using TheShop.SharedKernel;

namespace Ordering.Application.Commands;

public class ShipOrderCommandHandler : IRequestHandler<ShipOrderCommand, Result>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IOutboxService _outboxService;

    public ShipOrderCommandHandler(
        IRepository<Order> orderRepository,
        IOutboxService outboxService)
    {
        _orderRepository = orderRepository;
        _outboxService = outboxService;
    }

    public async Task<Result> Handle(ShipOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, ct);

        if (order is null)
        {
            return Result.Fail("Order not found");
        }

        var oldStatus = order.Status;
        var result = order.MarkAsShipped();
        if (result.IsFailed)
        {
            return result;
        }

        await _orderRepository.UpdateAsync(order, ct);

        // Save integration event in the same transaction as the order update
        var integrationEvent = new OrderStatusChangedIntegrationEvent(
            order.Id,
            oldStatus.ToString(),
            OrderStatus.Shipped.ToString());
        await _outboxService.SaveAsync(integrationEvent, ct);

        await _orderRepository.SaveChangesAsync(ct);

        return Result.Ok();
    }
}

