using System;

namespace Fastql
{
    /// <summary>
    /// Specifies the maximum length of string data. This is a metadata marker for validation purposes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MaxLengthAttribute : Attribute
    {
        public int Length { get; }

        public MaxLengthAttribute(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be non-negative.");

            Length = length;
        }
    }
}
