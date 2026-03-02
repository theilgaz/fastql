<div align="center">

[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/theilgaz/fastql/blob/master/LICENSE)
[![NuGet](https://img.shields.io/nuget/v/Fastql)](https://www.nuget.org/packages/Fastql/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Fastql)](https://www.nuget.org/packages/Fastql/)
[![.NET](https://img.shields.io/badge/.NET-6.0%20|%207.0%20|%208.0%20|%209.0-purple)](https://dotnet.microsoft.com/)

<img src="https://github.com/theilgaz/fastql/blob/main/resource/fastql-logo-resized.png?raw=true" width="250"/>

# Fastql

**SQL query builder for .NET entity classes**

Stop writing SQL strings by hand. Define your entity once, generate every query you need.
Works with Dapper, ADO.NET, or any micro-ORM.

</div>

---

## Jumpstart

```bash
dotnet add package Fastql
```

```csharp
using Fastql;

[Table("Users", "dbo")]
public class User
{
    [PK]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

```csharp
var sql = new FastqlBuilder<User>();

sql.InsertQuery();                        // INSERT INTO [dbo].[Users](Name, Email) VALUES(@Name, @Email)
sql.SelectQuery("Id = @Id");             // SELECT Id as Id, Name as Name, Email as Email FROM [dbo].[Users] WHERE Id = @Id;
sql.UpdateQuery(user, "Id = @Id");       // UPDATE [dbo].[Users] SET Name = @Name, Email = @Email WHERE Id = @Id;
sql.DeleteQuery("Id = @Id");             // DELETE FROM [dbo].[Users] WHERE Id = @Id;
sql.InsertQuery(returnIdentity: true);   // ...VALUES(@Name, @Email) ; SELECT SCOPE_IDENTITY();
sql.UpsertQuery();                       // MERGE INTO [dbo].[Users] AS target USING ...
```

That's it. Fastql reads your class, respects your attributes, and gives you the SQL.

---

## Table of Contents

- [Jumpstart](#jumpstart)
- [How to Start](#how-to-start)
  - [Step 1: Install](#step-1-install)
  - [Step 2: Pick a Database](#step-2-pick-a-database)
  - [Step 3: Decorate Your Entity](#step-3-decorate-your-entity)
  - [Step 4: Generate Queries](#step-4-generate-queries)
  - [Step 5: Wire to Dapper](#step-5-wire-to-dapper)
- [Two APIs, Same Output](#two-apis-same-output)
- [Attribute Reference](#attribute-reference)
- [Database Support](#database-support)
- [Features](#features)
  - [Fluent WHERE Builder](#fluent-where-builder)
  - [Validation](#validation)
  - [Bulk Operations](#bulk-operations)
  - [UPSERT / MERGE](#upsert--merge)
  - [PostgreSQL Type Casts](#postgresql-type-casts)
  - [Custom Exceptions](#custom-exceptions)
- [API Reference](#api-reference)
- [Migrating from v2 or v3](#migrating-from-v2-or-v3)
- [License](#license)

---

## How to Start

### Step 1: Install

```bash
dotnet add package Fastql
```

```xml
<PackageReference Include="Fastql" Version="4.0.0" />
```

### Step 2: Pick a Database

```csharp
// SQL Server is the default, no argument needed
var sql = new FastqlBuilder<User>();

// For other databases, pass the type
var sql = new FastqlBuilder<User>(DatabaseType.Postgres);
var sql = new FastqlBuilder<User>(DatabaseType.MySql);
var sql = new FastqlBuilder<User>(DatabaseType.SQLite);
var sql = new FastqlBuilder<User>(DatabaseType.Oracle);
```

### Step 3: Decorate Your Entity

At minimum, you need `[Table]` and `[PK]`. Everything else is optional.

```csharp
using Fastql;

[Table("Products", "inventory")]
public class Product
{
    [PK]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; }

    [Column("unit_price")]          // maps to a different column name
    public decimal Price { get; set; }

    [IsNotUpdatable]                // included in INSERT, excluded from UPDATE
    public DateTime CreatedAt { get; set; }

    [IsNotInsertable]               // excluded from INSERT, included in UPDATE
    public int StockCount { get; set; }

    [SelectOnly]                    // read-only, excluded from INSERT and UPDATE
    public decimal Discount { get; set; }

    [Ignore]                        // completely invisible to Fastql
    public Category? Category { get; set; }
}
```

### Step 4: Generate Queries

```csharp
var sql = new FastqlBuilder<Product>();

// INSERT (PK excluded automatically)
sql.InsertQuery();
// INSERT INTO [inventory].[Products](Name, unit_price, CreatedAt) VALUES(@Name, @Price, @CreatedAt)

// INSERT and get the new ID back
sql.InsertQuery(returnIdentity: true);
// ...VALUES(@Name, @Price, @CreatedAt) ; SELECT SCOPE_IDENTITY();

// UPDATE (PK, CreatedAt, SelectOnly excluded automatically)
var product = new Product { Id = 1, Name = "Widget", Price = 9.99m };
sql.UpdateQuery(product, "Id = @Id");
// UPDATE [inventory].[Products] SET Name = @Name, unit_price = @Price, StockCount = @StockCount WHERE Id = @Id;

// SELECT (Ignore excluded automatically)
sql.SelectQuery("Id = @Id");
// SELECT Id as Id, Name as Name, unit_price as Price, CreatedAt as CreatedAt, ... FROM [inventory].[Products] WHERE Id = @Id;

// DELETE
sql.DeleteQuery("Id = @Id");
// DELETE FROM [inventory].[Products] WHERE Id = @Id;
```

### Step 5: Wire to Dapper

Fastql generates Dapper-compatible `@Parameter` syntax. Plug it straight in:

```csharp
public class ProductRepository
{
    private readonly IDbConnection _db;
    private readonly FastqlBuilder<Product> _sql = new();

    public ProductRepository(IDbConnection db) => _db = db;

    public async Task<int> CreateAsync(Product p)
    {
        var query = _sql.InsertQuery(returnIdentity: true);
        return await _db.ExecuteScalarAsync<int>(query, p);
    }

    public async Task<Product?> GetAsync(int id)
    {
        var query = _sql.SelectQuery("Id = @Id");
        return await _db.QueryFirstOrDefaultAsync<Product>(query, new { Id = id });
    }

    public async Task UpdateAsync(Product p)
    {
        var query = _sql.UpdateQuery(p, "Id = @Id");
        await _db.ExecuteAsync(query, p);
    }

    public async Task DeleteAsync(int id)
    {
        var query = _sql.DeleteQuery("Id = @Id");
        await _db.ExecuteAsync(query, new { Id = id });
    }
}
```

---

## Two APIs, Same Output

Fastql gives you two ways to generate queries. Pick whichever fits your style:

**FastqlBuilder** is instance-based, great for DI and repositories:

```csharp
var sql = new FastqlBuilder<Product>(DatabaseType.Postgres);
sql.InsertQuery();
sql.SelectQuery("Id = @Id");
```

**FastqlHelper** is static, great for quick one-liners:

```csharp
FastqlHelper<Product>.DatabaseType = DatabaseType.Postgres;
FastqlHelper<Product>.InsertQuery();
FastqlHelper<Product>.SelectQuery("Id = @Id");
```

Both produce identical SQL. Every method on `FastqlBuilder` has a matching static method on `FastqlHelper`.

---

## Attribute Reference

### Table & Key

| Attribute | Where | What it does |
|-----------|-------|--------------|
| `[Table("name", "schema")]` | Class | Sets table name and schema. Default schema is `"dbo"`. |
| `[Table("name", "schema", OutputName.OnlyTable)]` | Class | Controls identifier format (see below). |
| `[PK]` or `[IsPrimaryKey]` | Property | Marks the primary key. Excluded from INSERT and UPDATE. |

**OutputName options:**

| Value | Output for `[Table("Users", "auth")]` |
|-------|---------------------------------------|
| `Default` | `[auth].[Users]` |
| `TableAndSchema` | `auth.Users` |
| `OnlyTable` | `Users` |

### Column Control

| Attribute | INSERT | UPDATE | SELECT | Use case |
|-----------|--------|--------|--------|----------|
| `[IsNotInsertable]` | excluded | included | included | Auto-set by trigger on insert |
| `[IsNotUpdatable]` | included | excluded | included | Immutable after creation |
| `[SelectOnly]` | excluded | excluded | included | Database-computed columns |
| `[Ignore]` | excluded | excluded | excluded | Navigation properties, computed C# props |
| `[CustomField]` | excluded | excluded | excluded | Alias for `[Ignore]` |

### Column Mapping

| Attribute | What it does |
|-----------|-------------|
| `[Field("col_name")]` | Maps property to a different column name |
| `[Field("col_name", FieldType.Jsonb)]` | Maps + adds PostgreSQL type cast |
| `[Column("col_name")]` | EF Core-style alias for `[Field]` |

### Validation

| Attribute | What it does |
|-----------|-------------|
| `[Required]` | Validated as non-null / non-whitespace by `Validate()` |
| `[MaxLength(n)]` | Validated as string length <= n by `Validate()` |
| `[DefaultValue(val)]` | Metadata marker (accessible via cached metadata) |

---

## Database Support

| Database | Identifier Style | Identity Return | Row Limit | Upsert |
|----------|------------------|-----------------|-----------|--------|
| **SQL Server** | `[schema].[table]` | `SELECT SCOPE_IDENTITY()` | `TOP(n)` | `MERGE` |
| **PostgreSQL** | `schema.table` | `RETURNING {pk}` | `LIMIT n` | `ON CONFLICT DO UPDATE` |
| **MySQL** | `schema.table` | `SELECT LAST_INSERT_ID()` | `LIMIT n` | `ON DUPLICATE KEY UPDATE` |
| **SQLite** | `schema.table` | `SELECT last_insert_rowid()` | `LIMIT n` | `INSERT OR REPLACE` |
| **Oracle** | `schema.table` | `RETURNING {pk} INTO :{pk}` | `FETCH FIRST n ROWS ONLY` | `MERGE ... FROM DUAL` |

Fastql generates the correct syntax per database automatically. Just pass `DatabaseType` to the constructor.

---

## Features

### Fluent WHERE Builder

Build WHERE clauses without string concatenation:

```csharp
using Fastql.Where;

// Simple
string w = Where.Column("Id").Equals("@Id");
// Id = @Id

// AND / OR chaining
string w = Where.Column("Active").Equals("@Active")
    .And.Column("Role").Equals("@Role")
    .Build();
// Active = @Active AND Role = @Role

string w = Where.Column("Status").Equals("@A")
    .Or.Column("Status").Equals("@B")
    .Build();
// Status = @A OR Status = @B

// Use directly with queries (implicit string conversion)
var sql = new FastqlBuilder<User>();
sql.SelectQuery(Where.Column("Id").Equals("@Id"));
sql.DeleteQuery(Where.Column("Id").Equals("@Id").And.Column("Active").Equals("@Active"));
```

**All operators:** `Equals`, `NotEquals`, `GreaterThan`, `LessThan`, `GreaterThanOrEqual`, `LessThanOrEqual`, `Like`, `In`, `IsNull`, `IsNotNull`, `Between`

### Validation

Enforce `[Required]` and `[MaxLength]` before hitting the database:

```csharp
using Fastql.Validation;

var product = new Product { Name = null! };

// Via builder
var result = sql.Validate(product);

// Or directly
var result = EntityValidator.Validate(product);

if (!result.IsValid)
{
    foreach (var err in result.Errors)
        Console.WriteLine($"{err.PropertyName}: {err.Message}");
    // Name: Name is required.
}

// Validate a single property
var result = EntityValidator.ValidateProperty(product, "Name");
```

### Bulk Operations

Insert or update many entities in one query:

```csharp
var users = new List<User>
{
    new() { Name = "Alice", Email = "alice@example.com" },
    new() { Name = "Bob",   Email = "bob@example.com" }
};

// Multi-row INSERT with indexed parameters
sql.BulkInsertQuery(users);
// INSERT INTO [dbo].[Users](Name, Email) VALUES(@Name_0, @Email_0), (@Name_1, @Email_1);

// Batch UPDATE (one statement per entity)
sql.BulkUpdateQuery(users);
// UPDATE [dbo].[Users] SET Name = @Name_0, Email = @Email_0 WHERE Id = @Id_0;
// UPDATE [dbo].[Users] SET Name = @Name_1, Email = @Email_1 WHERE Id = @Id_1;
```

Oracle bulk insert uses `INSERT ALL INTO ... SELECT 1 FROM DUAL` automatically.

### UPSERT / MERGE

Insert-or-update in a single query. Fastql picks the right syntax for your database:

```csharp
// PostgreSQL
var sql = new FastqlBuilder<User>(DatabaseType.Postgres);
sql.UpsertQuery();
// INSERT INTO public.Users(Name, Email) VALUES(@Name, @Email)
// ON CONFLICT (Id) DO UPDATE SET Name = @Name, Email = @Email;

// SQL Server
var sql = new FastqlBuilder<User>();
sql.UpsertQuery();
// MERGE INTO [dbo].[Users] AS target
// USING (SELECT @Id AS Id) AS source ON target.Id = source.Id
// WHEN MATCHED THEN UPDATE SET target.Name = @Name, target.Email = @Email
// WHEN NOT MATCHED THEN INSERT (Name, Email) VALUES(@Name, @Email);

// MySQL → ON DUPLICATE KEY UPDATE
// SQLite → INSERT OR REPLACE INTO
// Oracle → MERGE INTO ... USING (SELECT ... FROM DUAL)
```

### PostgreSQL Type Casts

When using PostgreSQL, add type casts to get proper data handling:

```csharp
[Table("events", "public", OutputName.TableAndSchema)]
public class Event
{
    [PK] public int Id { get; set; }

    public string Name { get; set; }

    [Field("event_data", FieldType.Jsonb)]       // → @EventData::jsonb
    public string EventData { get; set; }

    [Field("created_at", FieldType.Timestamp)]    // → @CreatedAt::timestamp
    public DateTime CreatedAt { get; set; }
}

var sql = new FastqlBuilder<Event>(DatabaseType.Postgres);
sql.InsertQuery(returnIdentity: true);
// INSERT INTO public.events(Name, event_data, created_at)
// VALUES(@Name, @EventData::jsonb, @CreatedAt::timestamp) RETURNING Id;
```

**Available casts:** `FieldType.Jsonb` (::jsonb), `FieldType.Timestamp` (::timestamp), `FieldType.Date` (::date), `FieldType.Time` (::time)

### Custom Exceptions

All Fastql exceptions inherit from `FastqlException` so you can catch them in one place:

```csharp
using Fastql.Exceptions;

try
{
    sql.InsertQuery();
}
catch (MissingParametersException)   { /* no insertable properties */ }
catch (MissingWhereClauseException)  { /* WHERE required but missing */ }
catch (DuplicateFieldException ex)   { /* field declared twice: ex.FieldName */ }
catch (FastqlException)              { /* catch-all for any Fastql error */ }
```

---

## API Reference

### FastqlBuilder&lt;TEntity&gt;

```csharp
var sql = new FastqlBuilder<TEntity>();                    // SQL Server
var sql = new FastqlBuilder<TEntity>(DatabaseType.Postgres); // other DB
```

| Method | Returns |
|--------|---------|
| `TableName()` | Formatted table name (`[dbo].[Users]`, `public.Users`, etc.) |
| `InsertQuery(returnIdentity)` | INSERT with `@Param` values. Pass `true` to append identity return. |
| `InsertStatement(returnIdentity)` | INSERT with `:Param` placeholders (positional). |
| `InsertReturnObjectQuery()` | INSERT that returns the full inserted row (`OUTPUT inserted.*` / `RETURNING *`). |
| `UpdateQuery(entity, where)` | UPDATE with actual values from the entity. |
| `UpdateStatement(entity, where)` | UPDATE with `:Param` placeholders. |
| `SelectQuery(where)` | SELECT all selectable columns with aliases. |
| `SelectQuery(columns, where, top)` | SELECT specific columns with row limit (TOP/LIMIT per DB). |
| `DeleteQuery(where)` | DELETE with WHERE clause. |
| `Validate(entity)` | Returns `ValidationResult` with errors for `[Required]` / `[MaxLength]`. |
| `BulkInsertQuery(entities)` | Multi-row INSERT with indexed params (`@Name_0`, `@Name_1`). |
| `BulkUpdateQuery(entities)` | Batch UPDATE statements, one per entity. |
| `UpsertQuery()` | Database-specific UPSERT (MERGE / ON CONFLICT / ON DUPLICATE KEY). |

### FastqlHelper&lt;TEntity&gt;

Same methods as above, but static:

```csharp
FastqlHelper<User>.DatabaseType = DatabaseType.Postgres;
FastqlHelper<User>.InsertQuery();
FastqlHelper<User>.TableName();   // public accessor
```

---

## Migrating from v2 or v3

### From v2 to v4

v2 used raw reflection on every call. v4 caches everything and adds new features.

**What you need to change:**

1. **Add `new()` constraint.** Your entities must have a parameterless constructor. This was always required at runtime but now it's enforced at compile time.

   ```csharp
   // v2: worked at runtime, no constraint
   // v4: compile-time constraint
   public class MyEntity { }  // ✓ parameterless constructor exists
   ```

2. **Nullable reference types.** Fastql now has `<Nullable>enable</Nullable>`. Mark nullable properties with `?`:

   ```csharp
   [Ignore]
   public Order? Order { get; set; }  // was: public Order Order { get; set; }
   ```

3. **Catch new exceptions.** v2 threw `System.Exception` and `DuplicateNameException`. v4 throws typed exceptions from `Fastql.Exceptions`:

   ```csharp
   // v2
   catch (Exception ex) when (ex.Message.Contains("parameters"))

   // v4
   catch (MissingParametersException) { }
   catch (MissingWhereClauseException) { }
   catch (DuplicateFieldException) { }
   ```

4. **`SetDatabaseType` is obsolete.** Use the property instead:

   ```csharp
   // v2
   FastqlHelper<User>.SetDatabaseType(DatabaseType.Postgres);

   // v4
   FastqlHelper<User>.DatabaseType = DatabaseType.Postgres;
   ```

**Everything else is additive.** Your existing `InsertQuery`, `UpdateQuery`, `SelectQuery`, and `DeleteQuery` calls work the same way.

### From v3 to v4

v3 to v4 is mostly painless. Here's what changed:

**Exception types (breaking if you catch exceptions):**

```csharp
// v3: System.Exception, DuplicateNameException
// v4: MissingParametersException, MissingWhereClauseException, DuplicateFieldException
```

**Postgres `RETURNING` now uses your actual PK name (breaking if you parsed the SQL):**

```csharp
// v3: RETURNING ID    (hardcoded uppercase)
// v4: RETURNING Id    (actual column name from your entity)
```

**Oracle identity return now generates SQL (breaking if you relied on no-op):**

```csharp
// v3: (no identity clause for Oracle)
// v4: RETURNING Id INTO :Id
```

**`SelectQuery(columns, where, top)` is now database-aware:**

```csharp
// v3: always SELECT TOP(n), wrong for Postgres/MySQL/SQLite/Oracle
// v4: TOP(n) for SQL Server, LIMIT n for Postgres/MySQL/SQLite, FETCH FIRST for Oracle
```

**`FastqlHelper.SelectQuery(where)` now uses QueryGenerator:**

```csharp
// v3: SELECT * FROM [dbo].[Users] WHERE ...  (raw string, no column aliases)
// v4: SELECT Id as Id, Name as Name, ... FROM [dbo].[Users] WHERE ...  (matches FastqlBuilder)
```

**Minor:**
- `SetDatabaseType()` / `GetDatabaseType()` → still work, marked `[Obsolete]`. Use `DatabaseType` property.
- `FastqlHelper.TableName()` is now `public`.
- `QueryBuilder` class deleted (was unused).
- `AddCondition` method removed from `FastQueryBuilder` (was dead code).

**New in v4 (no migration needed, just start using them):**
- `Validate(entity)` to enforce `[Required]` and `[MaxLength]`
- `BulkInsertQuery(entities)` / `BulkUpdateQuery(entities)` for batch operations
- `UpsertQuery()` for MERGE / ON CONFLICT per database
- `Where.Column("x").Equals("@x")` fluent WHERE builder
- Custom exceptions under `Fastql.Exceptions`

---

## Repository Pattern with Unit of Work

For a complete example using Fastql with the Repository Pattern and Unit of Work, check out the [sample project](https://github.com/theilgaz/fastql-unit-of-work-in-repository-pattern).

---

## License

MIT. See [LICENSE](LICENSE).

## Contributing

Contributions welcome. Open a PR or file an issue.

## Author

**Abdullah Ilgaz** | [GitHub](https://github.com/theilgaz)
