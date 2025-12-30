using System.Text.Json;
using TheShop.Infrastructure.Data;
using TheShop.SharedKernel;

namespace TheShop.Infrastructure.Services;

public class OutboxService : IOutboxService
{
    private readonly TheShopDbContext _context;

    public OutboxService(TheShopDbContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(IIntegrationEvent @event, CancellationToken ct = default)
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = @event.GetType().AssemblyQualifiedName ?? @event.GetType().Name,
            Content = JsonSerializer.Serialize(@event, @event.GetType()),
            OccurredOn = @event.OccurredOn
        };

        await _context.OutboxMessages.AddAsync(outboxMessage, ct);
    }
}

