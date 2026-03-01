using System;

namespace Fastql
{
    /// <summary>
    /// Marks a property as required. This is a metadata marker for validation purposes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class RequiredAttribute : Attribute
    {
    }
}
