using System.Collections.Generic;

namespace Fastql.Validation
{
    public sealed class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public IReadOnlyList<ValidationError> Errors { get; }

        public ValidationResult(IReadOnlyList<ValidationError> errors)
        {
            Errors = errors;
        }
    }
}
