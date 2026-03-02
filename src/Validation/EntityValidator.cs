using System;
using System.Collections.Generic;
using System.Linq;
using Fastql.Caching;

namespace Fastql.Validation
{
    public static class EntityValidator
    {
        public static ValidationResult Validate<TEntity>(TEntity entity) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var errors = new List<ValidationError>();

            foreach (var prop in metadata.Properties)
            {
                var propErrors = ValidateProperty(prop, entity);
                errors.AddRange(propErrors);
            }

            return new ValidationResult(errors);
        }

        public static ValidationResult ValidateProperty<TEntity>(TEntity entity, string propertyName) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var prop = metadata.Properties.FirstOrDefault(p => p.PropertyName == propertyName);

            if (prop == null)
                return new ValidationResult(Array.Empty<ValidationError>());

            var errors = ValidateProperty(prop, entity);
            return new ValidationResult(errors.ToList());
        }

        private static IEnumerable<ValidationError> ValidateProperty(PropertyMetadata prop, object entity)
        {
            if (prop.IsRequired)
            {
                var value = prop.GetValue(entity);
                if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
                {
                    yield return new ValidationError(prop.PropertyName, $"{prop.PropertyName} is required.");
                }
            }

            if (prop.MaxLength.HasValue)
            {
                var value = prop.GetValue(entity);
                if (value is string str && str.Length > prop.MaxLength.Value)
                {
                    yield return new ValidationError(prop.PropertyName,
                        $"{prop.PropertyName} must not exceed {prop.MaxLength.Value} characters.");
                }
            }
        }
    }
}
