# Database Schema

## Schemas

| Schema | Purpose |
|--------|---------|
| `identity` | User authentication |
| `catalog` | Products, brands, categories |
| `ordering` | Orders and line items |
| `outbox` | Event outbox pattern |

## Tables

### identity.Users

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| Email | varchar(255) | Unique, Not Null |
| PasswordHash | text | Not Null |
| FirstName | varchar(100) | Not Null |
| LastName | varchar(100) | Not Null |
| Role | int | Not Null |
| CreatedAt | timestamp | Not Null |
| IsActive | bool | Not Null |
| RefreshToken | text | Nullable |
| RefreshTokenExpiryTime | timestamp | Nullable |

### catalog.Brands

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| Name | varchar(100) | Unique, Not Null |

### catalog.Categories

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| Name | varchar(100) | Unique, Not Null |
| Description | varchar(500) | Nullable |

### catalog.Products

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| Name | varchar(200) | Not Null |
| Description | varchar(1000) | Nullable |
| Price | decimal(18,2) | Not Null |
| ImageUrl | text | Nullable |
| BrandId | uuid | FK → Brands |
| CategoryId | uuid | FK → Categories |
| IsAvailable | bool | Not Null |
| CreatedAt | timestamp | Not Null |

### ordering.Orders

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| BuyerId | uuid | Not Null |
| OrderDate | timestamp | Not Null |
| Status | int | Not Null |
| ShippingAddress_* | (owned) | Address value object |

### ordering.OrderItems

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| OrderId | uuid | FK → Orders |
| ProductId | uuid | Not Null |
| ProductName | varchar(200) | Not Null |
| UnitPrice | decimal(18,2) | Not Null |
| Quantity | int | Not Null |

### outbox.OutboxMessages

| Column | Type | Constraints |
|--------|------|-------------|
| Id | uuid | PK |
| Type | text | Not Null |
| Content | text | Not Null |
| OccurredOn | timestamp | Not Null |
| ProcessedOn | timestamp | Nullable |
| Error | text | Nullable |



