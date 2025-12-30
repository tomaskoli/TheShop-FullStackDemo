using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using System.ComponentModel;

namespace Chatbot.Infrastructure.Plugins;

public class DocumentSearchPlugin
{
    private readonly ISemanticTextMemory _memory;
    private readonly string _collectionName;

    public DocumentSearchPlugin(ISemanticTextMemory memory, string collectionName)
    {
        _memory = memory;
        _collectionName = collectionName;
    }

    [KernelFunction, Description("Search shop documents for terms and conditions, warranty, return policies, shipping info, and FAQ")]
    public async Task<string> SearchDocumentsAsync(
        [Description("The user question about shop policies, warranty, returns, shipping, or general FAQ")] string query,
        CancellationToken ct = default)
    {
        var results = new List<MemoryQueryResult>();
        
        await foreach (var result in _memory.SearchAsync(_collectionName, query, limit: 3, minRelevanceScore: 0.5, cancellationToken: ct))
        {
            results.Add(result);
        }

        if (results.Count == 0)
        {
            return "No relevant documents found in the knowledge base.";
        }

        return string.Join("\n\n---\n\n", results.Select(r => 
            $"Source: {r.Metadata.Description}\n{r.Metadata.Text}"));
    }
}

