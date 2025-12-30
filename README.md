# TheShop - Full-Stack Demo App

A showcase full-stack app built with **.NET 10** demonstrating Clean Architecture principles with Vertical Slices, responsive front-end, AI, CI/CD pipeline and enterprise patterns.

## Tech Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 10, ASP.NET Core Minimal APIs |
| Orchestration | .NET Aspire 13.1 |
| Database | PostgreSQL 18 |
| Cache | Redis 8 |
| Messaging | Apache Kafka |
| ORM | Entity Framework Core 10 |
| Frontend | Blazor Server |
| AI/RAG | Semantic Kernel, OpenAI, ChromaDB, Neo4j |
| Observability | Jaeger, Prometheus, Grafana, Aspire Dashboard |
| CI/CD | GitHub Actions and Container Registry (ghcr.io) |
| GitOps | ArgoCD |
| Deployment | Kubernetes (Rancher k8s), Helm |
| Containerization | Docker |

## Features

- **Modular Monolith** - Identity, Catalog, Basket, Ordering, and AI Chatbot modules
- **Clean Architecture** - Domain-driven design with separation of concerns
- **.NET Aspire** - Cloud-ready orchestration with service discovery and resilience
- **CQRS** - Command/Query separation using MediatR
- **Event-Driven** - Kafka messaging with Outbox pattern for reliability
- **ETL Pipeline** - Kafka Connect for PostgreSQL → Neo4j data synchronization
- **Observability** - OpenTelemetry with Aspire Dashboard, Jaeger, Prometheus, and Grafana
- **Security** - JWT authentication, RBAC, rate limiting, CORS, HTTPS enforcement
- **Caching** - Redis for basket storage, session data, and statistics caching
- **API Hardening** - Global exception handling, ProblemDetails responses
- **Admin Dashboard** - Order management, sales analytics, and user statistics with charts
- **AI Chatbot** - RAG-based assistant using Semantic Kernel, ChromaDB (vectore), and Neo4j (graph)
- **CI/CD** - GitHub Actions pipeline with ArgoCD GitOps deployment
- **robots.txt and llms.txt** - for crawlers and LLM context

## Architecture

```
                    ┌───────────────────────┐
                    │      DOMAIN LAYER     │  ← *.Domain projects
                    │  (Entities, Value     │     (Order, Product, ApplicationUser)
                    │   Objects, Events)    │
                    └───────────────────────┘
                              ▲
                              │
                    ┌───────────────────────┐
                    │   APPLICATION LAYER   │  ← *.Application projects
                    │  (Commands, Queries,  │     (Handlers, DTOs, Services interfaces)
                    │   Handlers, Services) │
                    └───────────────────────┘
                              ▲
          ┌───────────────────┴───────────────────┐
          │                                       │
┌─────────────────────┐             ┌─────────────────────┐
│    API LAYER        │             │  INFRASTRUCTURE     │
│  (Endpoints, DTOs)  │             │  (EF Core, Repos)   │
└─────────────────────┘             └─────────────────────┘
     TheShop.Api                    TheShop.Infrastructure
```

## Design Patterns

### Backend

| Pattern | Where Used | Purpose |
|---------|------------|---------|
| Aggregate Root | `Order.cs` | Domain consistency boundary |
| Value Object | `Address.cs` | Immutable domain concepts |
| Repository | `IRepository<T>`, `Repository<T>` | Data access abstraction |
| CQRS | `Commands/` and `Queries/` folders | Separate read/write models |
| Mediator | MediatR handlers | Decouple request/response |
| Pipeline Behavior | `ValidationBehavior` | Cross-cutting concerns |
| Outbox | `OutboxMessage`, `OutboxProcessor` | Reliable event publishing |
| Factory Method | `Order.Create()` | Encapsulated creation logic |
| Event-Driven | `IntegrationEvent`, Kafka | Async module communication |
| Idempotency | `IIdempotencyService`, Redis | Prevent duplicate order creation |
| Cache-Aside | `ISalesStatisticsCacheService` | Short-term statistics caching |

### Frontend (Blazor WebApp)

| Pattern | Where Used | Purpose |
|---------|------------|---------|
| Chain of Responsibility | `AuthorizationDelegatingHandler`, `TokenRefreshHandler` | HTTP request/response pipeline |
| Observer | `CustomAuthenticationStateProvider.OnUserStateChanged` | Reactive UI updates on state change |
| Provider | `CustomAuthenticationStateProvider` | Centralized authentication state |
| Partial Class | `TheShopApiClient.Partial.cs` | Extend NSwag-generated client |
| Lazy Loading | `Home.razor` + `IntersectionObserver` | Infinite scroll, on-demand data |
| Dispose | Components with `IDisposable`/`IAsyncDisposable` | Resource and subscription cleanup |
| Semaphore | `TokenRefreshHandler._refreshLock` | Prevent concurrent token refresh |

## Project Structure

```
TheShop/
├── src/
│   ├── TheShop.AppHost/          # .NET Aspire orchestrator
│   ├── TheShop.Api/              # Host application & endpoints
│   ├── Modules/
│   │   ├── Identity/             # User auth, JWT tokens, sessions
│   │   ├── Catalog/              # Products, brands, categories
│   │   ├── Basket/               # Shopping cart (Redis)
│   │   ├── Ordering/             # Orders & fulfillment
│   │   └── Chatbot/              # AI assistant (RAG with ChromaDB + Neo4j)
│   ├── Infrastructure/           # EF Core, services, messaging
│   │   ├── TheShop.Infrastructure/   # DB context, repositories, services
│   │   ├── TheShop.Messaging/        # Kafka, outbox pattern
│   │   └── TheShop.ServiceDefaults/  # Aspire defaults, OpenTelemetry, health checks
│   └── SharedKernel/             # Base classes, interfaces
├── clients/
│   └── TheShop.WebApp/           # Blazor Server frontend
├── tools/
│   └── ClientGenerator/          # NSwag-based API client generator
├── tests/                        # Unit & integration tests
└── deploy/                       # CI/CD pipeline, see the deploy/README.md.
```

## NuGet Packages

### Core & API
| Package | Version | Purpose |
|---------|---------|---------|
| MediatR | 14.0.0 | CQRS in-process messaging |
| FluentValidation | 12.1.1 | Request validation |
| FluentResults | 4.0.0 | Result pattern |
| Mapster | 7.4.0 | Object mapping |
| Swashbuckle.AspNetCore | 10.0.1 | Swagger/OpenAPI |
| Microsoft.AspNetCore.OpenApi | 10.0.1 | OpenAPI metadata generation |
| Newtonsoft.Json | 13.0.4 | JSON framework |

### Infrastructure
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.EntityFrameworkCore | 10.0.0 | ORM |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 | PostgreSQL provider |
| StackExchange.Redis | 2.10.1 | Redis client |
| Confluent.Kafka | 2.12.0 | Kafka client |

### Security
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.1 | JWT authentication |
| System.IdentityModel.Tokens.Jwt | 8.15.0 | JWT token handling |

### AI & Semantic Kernel
| Package | Version | Purpose |
|---------|---------|---------|
| Microsoft.SemanticKernel | 1.68.0 | AI orchestration & function calling |
| Microsoft.SemanticKernel.Connectors.OpenAI | 1.68.0 | OpenAI integration |
| Microsoft.SemanticKernel.Connectors.Chroma | 1.68.0-alpha | ChromaDB vector store |
| Neo4j.Driver | 5.28.4 | Neo4j graph database client |

### Observability
| Package | Version | Purpose |
|---------|---------|---------|
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.14.0 | OTLP exporter (Jaeger) |
| OpenTelemetry.Exporter.Prometheus.AspNetCore | 1.14.0-beta.1 | Prometheus metrics |
| OpenTelemetry.Extensions.Hosting | 1.14.0 | Host integration |
| OpenTelemetry.Instrumentation.AspNetCore | 1.14.0 | HTTP instrumentation |
| OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.0.0-beta.12 | EF Core instrumentation |
| OpenTelemetry.Instrumentation.Http | 1.14.0 | HTTP client instrumentation |
| OpenTelemetry.Instrumentation.Runtime | 1.14.0 | Runtime metrics |

### .NET Aspire
| Package | Version | Purpose |
|---------|---------|---------|
| Aspire.AppHost.Sdk | 13.1.0 | Aspire orchestration SDK |
| Microsoft.Extensions.ServiceDiscovery | 10.1.0 | Service discovery |
| Microsoft.Extensions.Http.Resilience | 10.1.0 | HTTP resilience policies |

### Health Checks
| Package | Version | Purpose |
|---------|---------|---------|
| AspNetCore.HealthChecks.NpgSql | 9.0.0 | PostgreSQL health check |
| AspNetCore.HealthChecks.Redis | 9.0.0 | Redis health check |
| AspNetCore.HealthChecks.Kafka | 9.0.0 | Kafka health check |
| AspNetCore.HealthChecks.UI.Client | 9.0.0 | Health check UI |

### Testing
| Package | Version | Purpose |
|---------|---------|---------|
| xunit | 2.9.3 | Test framework |
| xunit.runner.visualstudio | 3.1.5 | VS test runner |
| Microsoft.NET.Test.Sdk | 18.0.1 | Test SDK |
| NSubstitute | 5.3.0 | Mocking |
| FluentAssertions | 8.8.0 | Assertion library |
| Bogus | 35.6.5 | Fake data generation |
| coverlet.collector | 6.0.4 | Code coverage |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.1 | Integration testing |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.1 | In-memory DB for tests |

### Tooling
| Package | Version | Purpose |
|---------|---------|---------|
| NSwag.CodeGeneration.CSharp | 14.6.3 | API client generation |

## Security

### Authentication
- **JWT Bearer Tokens** - Stateless authentication using HS256 signed tokens
- **Access Token** - Short-lived (configurable, default 60 min)
- **Refresh Token** - Long-lived for token renewal (configurable, default 7 days)
- **Session Validation** - Each request validates session against Redis store

### Authorization (RBAC)
| Role | Permissions |
|------|-------------|
| **Customer** | Browse catalog, manage own basket, create orders, view own orders, cancel own orders |
| **Admin** | All Customer permissions + manage products, view all orders (paginated), ship orders, view sales/user statistics, access admin dashboard |

### JWT Token Claims
```json
{
  "sub": "user-guid",
  "email": "user@example.com",
  "name": "John Doe",
  "role": "Customer|Admin",
  "jti": "unique-token-id",
  "iat": "issued-at",
  "exp": "expiration"
}
```

### Session Management
- Sessions stored in Redis with device info and IP tracking
- Logout from single device or all devices
- View active sessions per user

### Rate Limiting
| Policy | Limit | Window | Applied To |
|--------|-------|--------|------------|
| **Global** | 100 requests | 1 minute | All authenticated endpoints |
| **Auth** | 10 requests | 5 minutes | Register, Refresh Token |
| **Login** | 5 requests | 15 minutes | Login endpoint (strict) |

## API Endpoints

### Identity (`/api/auth`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/register` | Register new user | Anonymous |
| POST | `/login` | User login, returns JWT tokens | Anonymous |
| GET | `/me` | Get current user profile | Required |
| POST | `/refresh` | Refresh access token | Anonymous |
| POST | `/logout` | Logout (single or all devices) | Required |
| GET | `/sessions` | Get user's active sessions | Required |
| GET | `/statistics` | User registration stats & active sessions | **Admin** |

### Catalog (`/api/catalog`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/products` | List products (paginated, filterable) | Anonymous |
| GET | `/products/{id}` | Get product by ID | Anonymous |
| POST | `/products` | Create new product | **Admin** |
| PUT | `/products/{id}` | Update product | **Admin** |
| DELETE | `/products/{id}` | Delete product | **Admin** |
| GET | `/brands` | List all brands | Anonymous |
| GET | `/categories` | List all categories | Anonymous |

### Basket (`/api/basket`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get current user's basket | Required |
| POST | `/items` | Add item to basket | Required |
| PUT | `/items/{productId}` | Update item quantity | Required |
| DELETE | `/items/{productId}` | Remove item from basket | Required |
| DELETE | `/` | Clear entire basket | Required |

### Orders (`/api/orders`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/` | Get current user's orders | Required |
| GET | `/{id}` | Get order by ID | Required |
| POST | `/` | Create order from basket | Required |
| PUT | `/{id}/cancel` | Cancel order (only if not shipped) | Required |
| PUT | `/{id}/ship` | Mark order as shipped | **Admin** |
| GET | `/all` | Get all orders (paginated) | **Admin** |
| GET | `/statistics` | Sales statistics by category/brand | **Admin** |

### Chatbot (`/api/chatbot`)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/chat` | Send message to AI assistant | Anonymous |

See [Chatbot Implementation Guide](doc/CHATBOT.md) for details.

### Health & Monitoring
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Full health check (all dependencies) |
| GET | `/alive` | Liveness probe |
| GET | `/ready` | Readiness probe |
| GET | `/metrics` | Prometheus metrics |

## Kafka Topics

Topics are auto-generated from integration event names (PascalCase → kebab-case):

| Topic | Publisher | Subscriber | Description |
|-------|-----------|------------|-------------|
| `product-price-changed` | Catalog | Basket | Notify price changes to update baskets |
| `order-created` | Ordering | - | New order notifications |
| `order-status-changed` | Ordering | - | Order status updates (e.g., shipped) |

### ETL Pipeline (PostgreSQL → Neo4j)

See [Kafka Connect ETL Guide](doc/KAFKA_CONNECT_ETL.md) for setup and configuration.

### Outbox Pattern
All integration events are saved to the `outbox.OutboxMessages` table within the same transaction as the business operation. A background processor (`OutboxProcessor`) polls for unprocessed messages and publishes them to Kafka, ensuring at-least-once delivery.

## Getting Started

### Prerequisites

- .NET 10 SDK
- Docker & Docker Compose

### Environment Variables (Production)

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=db;Database=theshopdb;Username=app;Password=<secret>` |
| `ConnectionStrings__Redis` | Redis connection string | `redis:6379,password=<secret>` |
| `Kafka__BootstrapServers` | Kafka broker address | `kafka:9092` |
| `Jwt__Secret` | JWT signing key (min 32 chars) | `<random-32+-character-string>` |
| `Cors__AllowedOrigins__0` | First allowed CORS origin | `https://yourfrontend.com` |
| `Cors__AllowedOrigins__1` | Additional CORS origin | `https://admin.yoursite.com` |
| `OpenTelemetry__OtlpEndpoint` | OTLP collector endpoint | `http://otel-collector:4317` |

### Run Infrastructure

```bash
# Start all dependencies
docker-compose -f deploy/Docker/docker-compose.postgres.yml up -d
docker-compose -f deploy/Docker/docker-compose.redis.yml up -d
docker-compose -f deploy/Docker/docker-compose.kafka.yml up -d
docker-compose -f deploy/Docker/docker-compose.observability.yml up -d
docker-compose -f deploy/Docker/docker-compose.neo4j.yml up -d
docker-compose -f deploy/Docker/docker-compose.chroma.yml up -d
```
For Kubernetes deployment with ArgoCD and GitOps, see the [K8s Deployment Guide](doc/KUBERNETES.md).

### Run Application

```bash
# Apply migrations
dotnet ef database update --project src/TheShop.Api

# Run with Aspire (orchestrates API + WebApp with dashboard)
aspire run

# Run API (standalone)
dotnet run --project src/TheShop.Api

# Run Web App
dotnet run --project clients/TheShop.WebApp
```

See [Mock data seed](deploy/scripts/seed-data.sql).

### Run Tests

```bash
dotnet test
```

### Generate API Client

```bash
dotnet run --project tools/ClientGenerator
```

## Web App Pages

| Route | Description | Access |
|-------|-------------|--------|
| `/` | Product catalog with search and filters | Public |
| `/login` | User authentication | Public |
| `/register` | New user registration | Public |
| `/basket` | Shopping cart management | Authenticated |
| `/checkout` | Order placement with shipping address | Authenticated |
| `/orders` | User's order history | Authenticated |
| `/orders/{id}` | Order details with cancel option | Authenticated |
| `/admin/orders` | All orders management with ship action | **Admin** |
| `/admin/reports` | Sales & user analytics with charts | **Admin** |

## URLs

| Service | URL | Purpose |
|---------|-----|---------|
| Aspire Dashboard | https://localhost:17168 | Unified logs, traces, metrics |
| Swagger | http://localhost:5232/swagger | API documentation |
| Web App | http://localhost:5233 | Blazor Server frontend |
| Jaeger | http://localhost:16686 | Distributed tracing |
| Prometheus | http://localhost:9090 | Metrics |
| Grafana | http://localhost:3000 | Dashboards |
| Kafka UI | http://localhost:8081 | Kafka management |
| Neo4j | http://localhost:7474 | Neo4j Browser |

## License

MIT License - see [LICENSE](LICENSE) for details.

---

*Built by [tomaskoli](https://github.com/tomaskoli)*
