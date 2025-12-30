# Kafka Connect ETL: PostgreSQL → Neo4j

Automatic data synchronization from PostgreSQL catalog tables to Neo4j graph database.

## Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   PostgreSQL    │────▶│      Kafka      │────▶│      Neo4j      │
│                 │     │                 │     │                 │
│ catalog.Brands  │     │  pg_brands      │     │  (:Brand)       │
│ catalog.Categor │     │  pg_categories  │     │  (:Category)    │
│ catalog.Products│     │  pg_products    │     │  (:Product)     │
└─────────────────┘     └─────────────────┘     └─────────────────┘
        ▲                       │                       │
        │                       │                       │
   JDBC Source              Topics              Neo4j Sink
   Connector                                    Connector
```

## Components

### Source Connectors (PostgreSQL → Kafka)

| Connector | Table | Topic |
|-----------|-------|-------|
| `pg-brands-source` | `catalog.Brands` | `pg_brands` |
| `pg-categories-source` | `catalog.Categories` | `pg_categories` |
| `pg-products-source` | `catalog.Products` | `pg_products` |

### Sink Connector (Kafka → Neo4j)

| Connector | Topics | Database |
|-----------|--------|----------|
| `neo4j-sink` | `pg_brands`, `pg_categories`, `pg_products` | `theshop` |

**Cypher Mappings:**

- **Brands**: `MERGE (b:Brand {id: event.Id}) SET b.name = event.Name`
- **Categories**: `MERGE (c:Category {id: event.Id}) SET c.name = event.Name, c.description = event.Description`
- **Products**: Creates Product node and relationships to Brand and Category

## Setup

### 1. Deploy Connectors

**PowerShell (Windows):**
```powershell
.\deploy\kafka-connect\deploy-connectors.ps1
```
### 2. Verify

```bash
# List connectors
curl http://localhost:8083/connectors

# Check connector status
curl http://localhost:8083/connectors/pg-products-source/status
curl http://localhost:8083/connectors/neo4j-sink/status

# View topics in Kafka UI
# http://localhost:8081
```

## Manual Connector Management

### Create a Connector

```bash
curl -X POST http://localhost:8083/connectors \
  -H "Content-Type: application/json" \
  -d @deploy/kafka-connect/connectors/pg-products-source.json
```

### Delete a Connector

```bash
curl -X DELETE http://localhost:8083/connectors/pg-products-source
```

### Pause/Resume a Connector

```bash
curl -X PUT http://localhost:8083/connectors/pg-products-source/pause
curl -X PUT http://localhost:8083/connectors/pg-products-source/resume
```

### Restart a Connector

```bash
curl -X POST http://localhost:8083/connectors/pg-products-source/restart
```

## Configuration Files

```
deploy/kafka-connect/
├── connectors/
│   ├── pg-brands-source.json      # Brands table source
│   ├── pg-categories-source.json  # Categories table source
│   ├── pg-products-source.json    # Products table source
│   └── neo4j-sink.json            # Neo4j sink (all topics)
├── deploy-connectors.ps1          # Windows deployment script
├── deploy-connectors.sh           # Linux/Mac deployment script
└── README.md                      # This file
```
