using System;

namespace Fastql
{
    /// <summary>
    /// Excludes a property from all query generation (INSERT, UPDATE, SELECT, DELETE).
    /// Use this for navigation properties, computed values, or any property that should not appear in SQL.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class IgnoreAttribute : Attribute
    {
    }
}
