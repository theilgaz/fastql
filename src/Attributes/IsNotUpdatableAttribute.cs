using System;

namespace Fastql.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class IsNotUpdatableAttribute : Attribute
    {
    }
}
