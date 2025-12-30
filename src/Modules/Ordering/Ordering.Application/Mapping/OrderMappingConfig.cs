using Mapster;
using Ordering.Application.Dtos;
using Ordering.Domain.Aggregates;

namespace Ordering.Application.Mapping;

public static class OrderMappingConfig
{
    private static bool _configured;

    public static void Configure()
    {
        if (_configured)
        {
            return;
        }

        TypeAdapterConfig<Order, OrderDto>.NewConfig()
            .Map(dest => dest.Items, src => src.OrderItems);

        _configured = true;
    }
}

