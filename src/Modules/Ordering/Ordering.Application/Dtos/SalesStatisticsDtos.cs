namespace Ordering.Application.Dtos;

public record SalesStatisticsDto(
    int TotalProductsSoldLastDay,
    int TotalProductsSoldLastMonth,
    decimal TotalRevenueLastDay,
    decimal TotalRevenueLastMonth,
    List<CategorySalesDto> SalesByCategory,
    List<BrandSalesDto> SalesByBrand);

public record CategorySalesDto(Guid CategoryId, string CategoryName, int QuantitySold, decimal Revenue);

public record BrandSalesDto(Guid BrandId, string BrandName, int QuantitySold, decimal Revenue);

