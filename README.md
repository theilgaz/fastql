<div align="center">

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/theilgaz/fastql/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Fastql)](https://www.nuget.org/packages/Fastql/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Fastql)](https://www.nuget.org/packages/Fastql/)
[![.NET](https://img.shields.io/badge/.NET-6.0%20|%207.0%20|%208.0%20|%209.0-purple)](https://dotnet.microsoft.com/)

<img src="https://github.com/theilgaz/fastql/blob/main/resource/fastql-logo-resized.png?raw=true" width="250"/>

# Fastql

**A lightweight, high-performance SQL query builder for .NET**

Generate type-safe CRUD queries from your entity classes. Perfect companion for Dapper and other micro-ORMs.

[Installation](#installation) •
[Quick Start](#quick-start) •
[Attributes](#attributes) •
[API Reference](#api-reference) •
[Examples](#examples)

</div>

---

## Features

- **Zero Dependencies** - Pure .NET library with no external dependencies
- **Multi-Database Support** - SQL Server, PostgreSQL, MySQL, SQLite, Oracle
- **High Performance** - Built-in metadata caching for reflection optimization
- **Type-Safe** - Generic constraints ensure compile-time safety
- **Dapper-Ready** - Generates `@Parameter` syntax compatible with Dapper
- **Nullable Support** - Full C# nullable reference types support
- **Flexible API** - Choose between instance-based builder or static helper

## Installation

### Package Manager
```bash
Install-Package Fastql
```

### .NET CLI
```bash
dotnet add package Fastql
```

### PackageReference
```xml
<PackageReference Include="Fastql" Version="3.0.1" />
```

## Quick Start

### 1. Define Your Entity

```csharp
using Fastql;

[Table("Customers", "Sales")]
public class Customer
{
    [IsPrimaryKey]
    public int Id { get; set; }

    public string Name { get; set; }

    public string Email { get; set; }

    [IsNotUpdatable]
    public DateTime CreatedAt { get; set; }
}
```

### 2. Generate Queries

```csharp
// Using FastqlBuilder (instance-based)
var fastql = new FastqlBuilder<Customer>();

string insertSql = fastql.InsertQuery();
// INSERT INTO [Sales].[Customers] ([Name],[Email],[CreatedAt]) VALUES (@Name,@Email,@CreatedAt)

string updateSql = fastql.UpdateQuery(customer, "Id = @Id");
// UPDATE [Sales].[Customers] SET [Name]=@Name,[Email]=@Email WHERE Id = @Id

string selectSql = fastql.SelectQuery("Id = @Id");
// SELECT [Name],[Email],[CreatedAt] FROM [Sales].[Customers] WHERE Id = @Id

string deleteSql = fastql.DeleteQuery("Id = @Id");
// DELETE FROM [Sales].[Customers] WHERE Id = @Id
```

```csharp
// Using FastqlHelper (static)
string insertSql = FastqlHelper<Customer>.InsertQuery();
string updateSql = FastqlHelper<Customer>.UpdateQuery(customer, "Id = @Id");
```

---

## Database Support

Fastql supports multiple database engines with appropriate syntax:

| Database | Identifier Style | Identity Return |
|----------|------------------|-----------------|
| **SQL Server** (default) | `[Schema].[Table]` | `SELECT SCOPE_IDENTITY()` |
| **PostgreSQL** | `Schema.Table` | `RETURNING ID` |
| **MySQL** | `Schema.Table` | - |
| **SQLite** | `Schema.Table` | - |
| **Oracle** | `Schema.Table` | - |

### Setting Database Type

```csharp
// FastqlBuilder
var fastql = new FastqlBuilder<Customer>(DatabaseType.Postgres);

// FastqlHelper
FastqlHelper<Customer>.SetDatabaseType(DatabaseType.Postgres);
```

---

## Attributes

Fastql uses attributes to control how your entities map to SQL queries.

### Table Mapping

#### `[Table]`
Defines the table name and schema for your entity.

```csharp
[Table("Customers")]                    // Uses default schema "dbo"
[Table("Customers", "Sales")]           // Explicit schema
[Table("Customers", "Sales", OutputName.OnlyTable)]  // Output: Customers
[Table("Customers", "Sales", OutputName.TableAndSchema)]  // Output: Sales.Customers
```

### Primary Key

#### `[IsPrimaryKey]` or `[PK]`
Marks the primary key column. Excluded from INSERT and UPDATE queries.

```csharp
[IsPrimaryKey]
public int Id { get; set; }

// Short form
[PK]
public int Id { get; set; }
```

### Column Control

#### `[IsNotInsertable]`
Excludes property from INSERT queries. Use for auto-generated or computed columns.

```csharp
[IsNotInsertable]
public DateTime UpdatedAt { get; set; }  // Set by database trigger
```

#### `[IsNotUpdatable]`
Excludes property from UPDATE queries. Use for immutable fields.

```csharp
[IsNotUpdatable]
public DateTime CreatedAt { get; set; }  // Should never change
```

#### `[SelectOnly]`
Excludes property from both INSERT and UPDATE queries. Included only in SELECT.

```csharp
[SelectOnly]
public string ComputedColumn { get; set; }  // Database-computed value
```

#### `[Ignore]`
Completely excludes property from all query generation.

```csharp
[Ignore]
public List<Order> Orders { get; set; }  // Navigation property

[Ignore]
public string DisplayName => $"{FirstName} {LastName}";  // Computed property
```

#### `[CustomField]`
Alias for `[Ignore]`. Excludes property from all queries.

```csharp
[CustomField]
public string FullAddress => $"{Street}, {City}, {Country}";
```

### Column Mapping

#### `[Field]` or `[Column]`
Maps a property to a differently-named database column.

```csharp
[Field("created_at")]
public DateTime CreatedAt { get; set; }

// With type cast (PostgreSQL)
[Field("metadata", FieldType.Jsonb)]
public string Metadata { get; set; }

// Using Column attribute (EF Core familiar syntax)
[Column("customer_name")]
public string Name { get; set; }
```

### Validation Metadata

#### `[Required]`
Marks a property as required (metadata marker).

```csharp
[Required]
public string Email { get; set; }
```

#### `[MaxLength]`
Specifies maximum string length (metadata marker).

```csharp
[MaxLength(100)]
public string Name { get; set; }
```

#### `[DefaultValue]`
Specifies a default value (metadata marker).

```csharp
[DefaultValue(true)]
public bool IsActive { get; set; }
```

---

## PostgreSQL Field Types

When using PostgreSQL, you can specify type casts for proper data handling:

```csharp
[Field("metadata", FieldType.Jsonb)]      // ::jsonb cast
public string Metadata { get; set; }

[Field("created_at", FieldType.Timestamp)] // ::timestamp cast
public DateTime CreatedAt { get; set; }

[Field("birth_date", FieldType.Date)]      // ::date cast
public DateTime BirthDate { get; set; }

[Field("start_time", FieldType.Time)]      // ::time cast
public TimeSpan StartTime { get; set; }
```

**Available FieldTypes:** `Initial`, `String`, `Int`, `Float`, `DateTime`, `Bool`, `Enum`, `Object`, `Jsonb`, `Timestamp`, `Date`, `Time`

---

## API Reference

### FastqlBuilder&lt;TEntity&gt;

Instance-based query builder with constructor injection support.

| Method | Description |
|--------|-------------|
| `TableName()` | Returns the formatted table name |
| `InsertQuery(returnIdentity)` | Generates INSERT query with `@Parameter` syntax |
| `InsertStatement(returnIdentity)` | Generates INSERT query with `:Parameter` syntax |
| `InsertReturnObjectQuery()` | INSERT query that returns the inserted row |
| `UpdateQuery(entity, where)` | Generates UPDATE query with values from entity |
| `UpdateStatement(entity, where)` | UPDATE query with `:Parameter` syntax |
| `SelectQuery(where)` | Generates SELECT query for all columns |
| `SelectQuery(columns, where, top)` | SELECT with specific columns and TOP limit |
| `DeleteQuery(where)` | Generates DELETE query |

### FastqlHelper&lt;TEntity&gt;

Static helper class for quick, one-off query generation.

```csharp
// Set database type globally for all entities
FastqlHelper<Customer>.SetDatabaseType(DatabaseType.Postgres);

// All the same methods as FastqlBuilder
FastqlHelper<Customer>.InsertQuery();
FastqlHelper<Customer>.UpdateQuery(entity, "Id = @Id");
FastqlHelper<Customer>.SelectQuery("IsActive = @IsActive");
FastqlHelper<Customer>.DeleteQuery("Id = @Id");
```

---

## Examples

### Complete Entity Definition

```csharp
using Fastql;

[Table("Products", "Inventory")]
public class Product
{
    [PK]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Column("description_text")]
    public string Description { get; set; }

    public decimal Price { get; set; }

    [DefaultValue(0)]
    public int StockQuantity { get; set; }

    [IsNotUpdatable]
    public DateTime CreatedAt { get; set; }

    [IsNotInsertable]
    public DateTime? UpdatedAt { get; set; }

    [SelectOnly]
    public decimal CalculatedDiscount { get; set; }

    [Ignore]
    public Category Category { get; set; }

    [CustomField]
    public string DisplayPrice => $"${Price:F2}";
}
```

### Dapper Integration

```csharp
public class ProductRepository
{
    private readonly IDbConnection _connection;
    private readonly FastqlBuilder<Product> _fastql;

    public ProductRepository(IDbConnection connection)
    {
        _connection = connection;
        _fastql = new FastqlBuilder<Product>();
    }

    // CREATE
    public async Task<int> CreateAsync(Product product)
    {
        var sql = _fastql.InsertQuery(returnIdentity: true);
        return await _connection.ExecuteScalarAsync<int>(sql, product);
    }

    // READ
    public async Task<Product?> GetByIdAsync(int id)
    {
        var sql = _fastql.SelectQuery("Id = @Id");
        return await _connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Product>> GetActiveAsync()
    {
        var sql = _fastql.SelectQuery("StockQuantity > @MinStock");
        return await _connection.QueryAsync<Product>(sql, new { MinStock = 0 });
    }

    // UPDATE
    public async Task<bool> UpdateAsync(Product product)
    {
        var sql = _fastql.UpdateQuery(product, "Id = @Id");
        var affected = await _connection.ExecuteAsync(sql, product);
        return affected > 0;
    }

    // DELETE
    public async Task<bool> DeleteAsync(int id)
    {
        var sql = _fastql.DeleteQuery("Id = @Id");
        var affected = await _connection.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
}
```

### PostgreSQL with JSONB

```csharp
[Table("events", "public")]
public class Event
{
    [PK]
    public int Id { get; set; }

    public string Name { get; set; }

    [Field("event_data", FieldType.Jsonb)]
    public string EventData { get; set; }

    [Field("created_at", FieldType.Timestamp)]
    public DateTime CreatedAt { get; set; }
}

// Usage
var fastql = new FastqlBuilder<Event>(DatabaseType.Postgres);
var insertSql = fastql.InsertQuery(returnIdentity: true);
// INSERT INTO public.events (Name,event_data,created_at)
// VALUES (@Name,@EventData::jsonb,@CreatedAt::timestamp) RETURNING ID;
```

### Repository Pattern with Unit of Work

For a complete example using Fastql with Repository Pattern and Unit of Work, check out the [sample project](https://github.com/theilgaz/fastql-unit-of-work-in-repository-pattern).

---

## Migration from v2.x to v3.0

### Breaking Changes

1. **Generic Constraint Added**: `FastqlBuilder<TEntity>` and `FastqlHelper<TEntity>` now require `where TEntity : class, new()`

2. **Nullable Reference Types**: The library now uses nullable reference types. Update your entity properties accordingly.

### New Features in v3.0

- Multi-target framework support (.NET 6, 7, 8, 9)
- New attributes: `[Column]`, `[Ignore]`, `[Required]`, `[MaxLength]`, `[DefaultValue]`
- Additional database types: MySQL, SQLite, Oracle
- Performance improvements with metadata caching

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Author

**Abdullah Ilgaz** - [GitHub](https://github.com/theilgaz)
