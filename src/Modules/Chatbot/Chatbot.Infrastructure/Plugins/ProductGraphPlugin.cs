using Microsoft.SemanticKernel;
using Neo4j.Driver;
using System.ComponentModel;

namespace Chatbot.Infrastructure.Plugins;

public class ProductGraphPlugin
{
    private readonly IDriver _driver;
    private readonly string _database;

    public ProductGraphPlugin(IDriver driver, string database)
    {
        _driver = driver;
        _database = database;
    }

    [KernelFunction, Description("Find available products by name, brand or category. Use this when user asks about specific products, brands like Apple/Samsung, or categories like Smartphones/Laptops/Headsets")]
    public async Task<string> FindProductsAsync(
        [Description("Product name, brand name, or category name to search for")] string searchTerm,
        CancellationToken ct = default)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));
        
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Product)
                WHERE p.isAvailable = true
                  AND (toLower(p.name) CONTAINS toLower($search)
                       OR EXISTS { (p)-[:BELONGS_TO]->(c:Category) WHERE toLower(c.name) CONTAINS toLower($search) }
                       OR EXISTS { (p)-[:MADE_BY]->(b:Brand) WHERE toLower(b.name) CONTAINS toLower($search) })
                OPTIONAL MATCH (p)-[:BELONGS_TO]->(cat:Category)
                OPTIONAL MATCH (p)-[:MADE_BY]->(brand:Brand)
                RETURN p.name AS name, p.description AS description, p.price AS price, 
                       cat.name AS category, brand.name AS brand
                ORDER BY p.price
                LIMIT 5",
                new { search = searchTerm });
            
            return await cursor.ToListAsync();
        });

        if (result.Count == 0)
        {
            return "No available products found matching your search.";
        }

        return "Found products:\n" + string.Join("\n", result.Select(r => 
            $"- {r["name"]} ({r["brand"]}) - {r["category"]} - ${r["price"]}\n  {r["description"]}"));
    }

    [KernelFunction, Description("Get product recommendations similar to a given product. Use this when user asks for alternatives, similar products, or recommendations")]
    public async Task<string> GetRecommendationsAsync(
        [Description("Product name to get recommendations for")] string productName,
        CancellationToken ct = default)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));
        
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Product)-[:BELONGS_TO]->(cat:Category)<-[:BELONGS_TO]-(similar:Product)
                WHERE toLower(p.name) CONTAINS toLower($name) 
                  AND p <> similar 
                  AND similar.isAvailable = true
                OPTIONAL MATCH (similar)-[:MADE_BY]->(brand:Brand)
                RETURN DISTINCT similar.name AS name, similar.description AS description, 
                       similar.price AS price, cat.name AS category, brand.name AS brand
                ORDER BY similar.price
                LIMIT 5",
                new { name = productName });
            
            return await cursor.ToListAsync();
        });

        if (result.Count == 0)
        {
            return "No similar products found. Try searching for products in a specific category.";
        }

        return "Recommended similar products:\n" + string.Join("\n", result.Select(r => 
            $"- {r["name"]} ({r["brand"]}) - ${r["price"]}\n  {r["description"]}"));
    }

    [KernelFunction, Description("Get products from a specific brand. Use when user asks what products a brand offers")]
    public async Task<string> GetProductsByBrandAsync(
        [Description("Brand name to get products for")] string brandName,
        CancellationToken ct = default)
    {
        await using var session = _driver.AsyncSession(o => o.WithDatabase(_database));
        
        var result = await session.ExecuteReadAsync(async tx =>
        {
            var cursor = await tx.RunAsync(@"
                MATCH (p:Product)-[:MADE_BY]->(b:Brand)
                WHERE toLower(b.name) CONTAINS toLower($brand) AND p.isAvailable = true
                OPTIONAL MATCH (p)-[:BELONGS_TO]->(cat:Category)
                RETURN p.name AS name, p.description AS description, p.price AS price, 
                       cat.name AS category, b.name AS brand
                ORDER BY cat.name, p.price
                LIMIT 10",
                new { brand = brandName });
            
            return await cursor.ToListAsync();
        });

        if (result.Count == 0)
        {
            return $"No products found for brand '{brandName}'.";
        }

        return $"Products from {brandName}:\n" + string.Join("\n", result.Select(r => 
            $"- {r["name"]} - {r["category"]} - ${r["price"]}\n  {r["description"]}"));
    }
}

