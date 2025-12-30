namespace Chatbot.Infrastructure.Settings;

public class ChatbotSettings
{
    public string OpenAiApiKey { get; set; } = string.Empty;
    public string OpenAiModel { get; set; } = "gpt-4o-mini";
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public string SystemPrompt { get; set; } = string.Empty;
    public string ChromaEndpoint { get; set; } = "http://localhost:8100";
    public string ChromaCollectionName { get; set; } = "theshop_documents";
    public string Neo4jUri { get; set; } = "neo4j://localhost:7687";
    public string Neo4jUser { get; set; } = "neo4j";
    public string Neo4jPassword { get; set; } = string.Empty;
    public string Neo4jDatabase { get; set; } = "theshop";
}

