using Fastql;

namespace Fastql.Tests;

[Table("Customers", "dbo")]
public class Customer
{
    [PK]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [IsNotUpdatable]
    public DateTime CreatedAt { get; set; }

    [SelectOnly]
    public DateTime? LastLoginAt { get; set; }

    [CustomField]
    public string FullDescription => $"{Name} ({Email})";
}

[Table("Products", "inventory", OutputName.TableAndSchema)]
public class Product
{
    [IsPrimaryKey]
    public int ProductId { get; set; }

    [Field("product_name")]
    public string Name { get; set; } = string.Empty;

    [Field("unit_price")]
    public decimal Price { get; set; }

    [IsNotInsertable]
    public int StockCount { get; set; }
}

[Table("Orders", "sales", OutputName.OnlyTable)]
public class Order
{
    [PK]
    public int OrderId { get; set; }

    public int CustomerId { get; set; }

    public decimal TotalAmount { get; set; }

    [Ignore]
    public Customer? Customer { get; set; }
}

[Table("Settings", "config")]
public class Setting
{
    [PK]
    public int Id { get; set; }

    [Column("setting_key")]
    public string Key { get; set; } = string.Empty;

    [Column("setting_value")]
    public string Value { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Description { get; set; } = string.Empty;

    [DefaultValue(true)]
    public bool IsActive { get; set; }
}

[Table("logs", "public", OutputName.TableAndSchema)]
public class PostgresLog
{
    [PK]
    public int Id { get; set; }

    public string Message { get; set; } = string.Empty;

    [Field("metadata", FieldType.Jsonb)]
    public string Metadata { get; set; } = string.Empty;

    [Field("created_at", FieldType.Timestamp)]
    public DateTime CreatedAt { get; set; }
}
