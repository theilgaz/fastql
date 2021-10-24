using Fastql.Attributes;
using Fastql.Utilities;
using System;
using System.Linq;
using System.Reflection;

namespace Fastql
{
    /// <summary>
    /// A small and fast library for building SQL queries from entity classes
    /// in a better way than regular string concatenation.
    /// </summary>
    public class FastqlBuilder<TEntity>
    {
        private readonly TEntity _entity;

        public FastqlBuilder()
        {
            _entity = (TEntity) Activator.CreateInstance(typeof(TEntity));
        }

        public string TableName()
        {
            var type = _entity.GetType();
            if (type.CustomAttributes.Count() > 0)
            {
                var attribute = type.CustomAttributes.FirstOrDefault();
                if (attribute.AttributeType.Name == "TableAttribute")
                {
                    TableAttribute table = (TableAttribute) Attribute.GetCustomAttribute(type, typeof(TableAttribute));
                    return $"[{table.Schema}].[{table.TableName}]";
                }
            }

            return "";
        }

        public string InsertQuery(bool returnIdentity = false)
        {
            QueryBuilder qb = new QueryBuilder(TableName());
            foreach (var propertyInfo in _entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)))
                {
                    qb.AddIdentityColumn(propertyInfo.Name);
                }

                if ((!Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute))) &&
                    (!Attribute.IsDefined(propertyInfo, typeof(IsNotInsertableAttribute))))
                {
                    qb.Add(propertyInfo.Name, propertyInfo.GetValue(_entity));
                }
            }

            return (returnIdentity) ? qb.InsertSql + "SELECT SCOPE_IDENTITY();" : qb.InsertSql;
        }

        public string UpdateQuery(TEntity entity, string where)
        {
            QueryBuilder qb = new QueryBuilder(TableName(), $" WHERE {where}");
            foreach (var propertyInfo in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if ((!Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute))) &&
                    (!Attribute.IsDefined(propertyInfo, typeof(IsNotUpdatableAttribute))))
                {
                    qb.Add(propertyInfo.Name, propertyInfo.GetValue(entity));
                }
            }

            return qb.UpdateSql;
        }

        public string DeleteQuery(string where)
        {
            return $"DELETE FROM {TableName()} WHERE {where};";
        }

        public string SelectQuery(string where)
        {
            return $"SELECT * FROM {TableName()} WHERE {where};";
        }

        /// <summary>
        /// Generates SELECT query with desired columns, where conditions and top records.
        /// </summary>
        /// <param name="columns">If its length greater than zero (0), it'll be included. Otherwise all columns will be fetched.</param>
        /// <param name="where"></param>
        /// <param name="top">Default 1000 rows will be fetched.</param>
        /// <returns></returns>
        public string SelectQuery(string[] columns, string where, int top = 1000)
        {
            string s = string.Format("[{0}]", string.Join("],[", columns.Select(i => i.Replace("[", ""))));
            return columns is {Length: > 0}
                ? $"SELECT TOP({top}) {string.Join(",", s)} FROM {TableName()} WHERE {@where};"
                : $"SELECT TOP({top}) * FROM {TableName()} WHERE {@where};";
        }
    }
}