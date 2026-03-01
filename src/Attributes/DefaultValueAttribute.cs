using System;

namespace Fastql
{
    /// <summary>
    /// Specifies a default value for a property. This is a metadata marker for validation or documentation purposes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DefaultValueAttribute : Attribute
    {
        public object? Value { get; }

        public DefaultValueAttribute(object? value)
        {
            Value = value;
        }
    }
}
