using System;

namespace Fastql
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class SelectOnlyAttribute : Attribute
    {
    }
}