using System;
using System.Collections.Concurrent;

namespace Fastql.Caching
{
    /// <summary>
    /// Thread-safe cache for entity type metadata to avoid repeated reflection.
    /// </summary>
    public static class TypeMetadataCache
    {
        private static readonly ConcurrentDictionary<Type, EntityMetadata> Cache = new();

        /// <summary>
        /// Gets or creates cached metadata for the specified entity type.
        /// </summary>
        public static EntityMetadata GetOrCreate<TEntity>() where TEntity : class
        {
            return Cache.GetOrAdd(typeof(TEntity), type => new EntityMetadata(type));
        }

        /// <summary>
        /// Gets or creates cached metadata for the specified type.
        /// </summary>
        public static EntityMetadata GetOrCreate(Type entityType)
        {
            return Cache.GetOrAdd(entityType, type => new EntityMetadata(type));
        }

        /// <summary>
        /// Clears all cached metadata. Useful for testing or when entity definitions change dynamically.
        /// </summary>
        public static void Clear()
        {
            Cache.Clear();
        }

        /// <summary>
        /// Removes cached metadata for a specific entity type.
        /// </summary>
        public static bool Remove<TEntity>() where TEntity : class
        {
            return Cache.TryRemove(typeof(TEntity), out _);
        }

        /// <summary>
        /// Removes cached metadata for a specific type.
        /// </summary>
        public static bool Remove(Type entityType)
        {
            return Cache.TryRemove(entityType, out _);
        }

        /// <summary>
        /// Gets the number of types currently cached.
        /// </summary>
        public static int Count => Cache.Count;
    }
}
