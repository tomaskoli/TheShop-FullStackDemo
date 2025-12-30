using Chatbot.Application.Services;
using Chatbot.Infrastructure.Plugins;
using Chatbot.Infrastructure.Services;
using Chatbot.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Chroma;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Neo4j.Driver;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0020

namespace Chatbot.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddChatbot(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("Chatbot").Get<ChatbotSettings>()!;
        services.Configure<ChatbotSettings>(configuration.GetSection("Chatbot"));

        services.AddSingleton<IDriver>(_ =>
            GraphDatabase.Driver(settings.Neo4jUri, AuthTokens.Basic(settings.Neo4jUser, settings.Neo4jPassword)));

        services.AddSingleton<ISemanticTextMemory>(sp =>
        {
            var memoryBuilder = new MemoryBuilder();
            memoryBuilder.WithOpenAITextEmbeddingGeneration(settings.EmbeddingModel, settings.OpenAiApiKey);
            memoryBuilder.WithChromaMemoryStore(settings.ChromaEndpoint);
            return memoryBuilder.Build();
        });

        services.AddSingleton(sp =>
        {
            var builder = Kernel.CreateBuilder();
            
            builder.AddOpenAIChatCompletion(settings.OpenAiModel, settings.OpenAiApiKey);

            var driver = sp.GetRequiredService<IDriver>();
            var memory = sp.GetRequiredService<ISemanticTextMemory>();
            
            builder.Plugins.AddFromObject(new ProductGraphPlugin(driver, settings.Neo4jDatabase), "ProductGraph");
            builder.Plugins.AddFromObject(new DocumentSearchPlugin(memory, settings.ChromaCollectionName), "DocumentSearch");

            return builder.Build();
        });

        services.AddScoped<IChatbotService, ChatbotService>();

        return services;
    }
}

