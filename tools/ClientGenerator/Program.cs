using NSwag;
using NSwag.CodeGeneration.CSharp;

Console.WriteLine("Generating API Client...");

var document = await OpenApiDocument.FromUrlAsync("http://localhost:5232/openapi/v1.json");

var generator = new CSharpClientGenerator(document, new CSharpClientGeneratorSettings
{
    ClassName = "TheShopApiClient",
    CSharpGeneratorSettings = { Namespace = "TheShop.WebApp.Services" },
    InjectHttpClient = true
});

var outputPath = Path.Combine(AppContext.BaseDirectory, "../../../../../clients/TheShop.WebApp/Services/TheShopApiClient.cs");
Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
await File.WriteAllTextAsync(outputPath, generator.GenerateFile());

Console.WriteLine("API Client generated successfully!");

