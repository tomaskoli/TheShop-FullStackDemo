# TheShop AI Chatbot Guide

An AI-powered assistant using **Retrieval-Augmented Generation (RAG)** with Microsoft Semantic Kernel.

## Architecture Overview

```
┌───────────────────────────────────────────────────────────────────────┐
│                           User Question                               │
└───────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌───────────────────────────────────────────────────────────────────────┐
│                         Semantic Kernel                               │
│  ┌─────────────────────────────────────────────────────────────────┐  │
│  │                    OpenAI GPT-5-nano                            │  │
│  │              (Function Calling / Tool Use)                      │  │
│  └─────────────────────────────────────────────────────────────────┘  │
│                   │                               │                   │
│         ┌─────────┴─────────┐            ┌────────┴────────┐          │
│         ▼                   ▼            ▼                 ▼          │
│  ┌─────────────────┐  ┌──────────────────────────────────────┐        │
│  │ DocumentSearch  │  │         ProductGraph                 │        │
│  │    Plugin       │  │           Plugin                     │        │
│  │  (ChromaDB)     │  │          (Neo4j)                     │        │
│  └────────┬────────┘  └──────────────┬───────────────────────┘        │
└───────────┼──────────────────────────┼────────────────────────────────┘
            │                          │
            ▼                          ▼
┌───────────────────────┐    ┌───────────────────────────────┐
│      ChromaDB         │    │           Neo4j               │
│   (Vector Store)      │    │       (Graph Database)        │
│                       │    │                               │
│ • Warranty policies   │    │  ┌─────────┐                  │
│ • Return policies     │    │  │ Product │                  │
│ • Shipping info       │    │  └────┬────┘                  │
│ • FAQ                 │    │       │                       │
│ • Terms & conditions  │    │  BELONGS_TO    MADE_BY        │
│                       │    │       │           │           │
└───────────────────────┘    │  ┌────▼────┐ ┌───▼───┐        │
                             │  │Category │ │ Brand │        │
                             │  └─────────┘ └───────┘        │
                             └───────────────────────────────┘
```

## RAG Flow

```
User: "What is your warranty policy?"
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ 1. Semantic Kernel sends question + function definitions    │
│    to OpenAI                                                │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ 2. OpenAI decides to call DocumentSearch plugin             │
│    Returns: { "function": "SearchDocumentsAsync",           │
│               "arguments": { "query": "warranty policy" }}  │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ 3. Semantic Kernel executes the function locally:           │
│    - Generates embedding for "warranty policy"              │
│    - Searches ChromaDB for similar documents                │
│    - Returns matching policy text                           │
└─────────────────────────────────────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────────────────────────────┐
│ 4. OpenAI generates final response using retrieved context  │
│    "TheShop provides a 24-month warranty covering..."       │
└─────────────────────────────────────────────────────────────┘
```

## Project Structure

```
src/Modules/Chatbot/
├── Chatbot.Application/
│   ├── Commands/
│   │   ├── ChatCommand.cs              # MediatR command
│   │   └── ChatCommandHandler.cs       # Command handler
│   ├── Dtos/
│   │   └── ChatDtos.cs                 # Request/Response DTOs
│   ├── Services/
│   │   └── IChatbotService.cs          # Service interface
│   └── Validators/
│       └── ChatCommandValidator.cs     # FluentValidation
│
└── Chatbot.Infrastructure/
    ├── Plugins/
    │   ├── DocumentSearchPlugin.cs     # ChromaDB RAG search
    │   └── ProductGraphPlugin.cs       # Neo4j graph queries
    ├── Services/
    │   └── ChatbotService.cs           # Semantic Kernel orchestration
    ├── Settings/
    │   └── ChatbotSettings.cs          # Configuration
    └── DependencyInjection.cs          # Service registration
```

## Plugins

### DocumentSearchPlugin (ChromaDB)

Searches shop documents using vector similarity:

```csharp
[KernelFunction, Description("Search shop documents for terms and conditions, warranty, return policies, shipping info, and FAQ")]
public async Task<string> SearchDocumentsAsync(
    [Description("The user question about shop policies")] string query,
    CancellationToken ct = default)
```

### ProductGraphPlugin (Neo4j)

Queries product relationships in the graph database:

```csharp
[KernelFunction, Description("Find available products by name, brand or category")]
public async Task<string> FindProductsAsync(string searchTerm, CancellationToken ct)

[KernelFunction, Description("Get product recommendations similar to a given product")]
public async Task<string> GetRecommendationsAsync(string productName, CancellationToken ct)

[KernelFunction, Description("Get products from a specific brand")]
public async Task<string> GetProductsByBrandAsync(string brandName, CancellationToken ct)
```

**Graph Schema:**
```cypher
(:Product {id, name, description, price, isAvailable})
    -[:BELONGS_TO]->(:Category {id, name})
    -[:MADE_BY]->(:Brand {id, name})
```

## Setup

### 1. Seed Neo4j

In Neo4j Browser (http://localhost:7474):

```cypher
-- Create database
CREATE DATABASE theshop IF NOT EXISTS;
:use theshop

-- Run seed script from deploy/scripts/seed-neo4j.cypher
```

### 2. Seed ChromaDB

```powershell
pip install chromadb openai
$env:OPENAI_API_KEY = "your-api-key"
python deploy/scripts/seed-chroma.py
```

### 3. Configure API

Set your OpenAI API key in `appsettings.json`.

```json
{
  "Chatbot": {
    "OpenAiApiKey": "your-api-key",
    "OpenAiModel": "gpt-5-nano",
    "EmbeddingModel": "text-embedding-3-small",
    "SystemPrompt": "You are a helpful shopping assistant for TheShop...",
    "ChromaEndpoint": "http://localhost:8100",
    "ChromaCollectionName": "theshop_documents",
    "Neo4jUri": "neo4j://localhost:7687",
    "Neo4jUser": "neo4j",
    "Neo4jPassword": "neo4j-pw",
    "Neo4jDatabase": "theshop"
  }
}
```

### Environment Variables

| Variable | Description |
|----------|-------------|
| `Chatbot__OpenAiApiKey` | OpenAI API key |
| `Chatbot__Neo4jPassword` | Neo4j password |