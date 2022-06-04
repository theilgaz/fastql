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
        private readonly DatabaseType _databaseType = DatabaseType.SqlServer;

        public FastqlBuilder()
        {
            _entity = (TEntity) Activator.CreateInstance(typeof(TEntity));
        }

        public FastqlBuilder(DatabaseType databaseType)
        {
            _entity = (TEntity) Activator.CreateInstance(typeof(TEntity));
            _databaseType = databaseType;
        }
        
        public string TableName()
        {
            var type = _entity.GetType();
            if (type.CustomAttributes.Any())
            {
                var attribute = type.CustomAttributes.FirstOrDefault(x=>x.AttributeType == typeof(TableAttribute));
                if (attribute.AttributeType.Name == "TableAttribute")
                {
                    var table = (TableAttribute) Attribute.GetCustomAttribute(type, typeof(TableAttribute));
                    return table.Output switch
                    {
                        OutputName.OnlyTable => table.TableName,
                        OutputName.TableAndSchema => table.Schema + "." + table.TableName,
                        _ => $"[{table.Schema}].[{table.TableName}]"
                    };
                }
            }

            return "";
        }

        public string InsertQuery(bool returnIdentity = false)
        {
            var qb = new QueryBuilder(TableName());
            foreach (var propertyInfo in _entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) || Attribute.IsDefined(propertyInfo, typeof(PKAttribute)))
                {
                    continue;
                }
                {
                    qb.AddIdentityColumn(propertyInfo.Name);
                }

                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotInsertableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute))) continue;
                
                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    qb.Add(field.FieldName, propertyInfo.GetValue(_entity));
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.GetValue(_entity));
                }
            }

            if (_databaseType == DatabaseType.Postgres)
            {
                return returnIdentity? qb.InsertSql + "SELECT LASTVAL(); " : qb.InsertSql;
            }
            
            return returnIdentity ? qb.InsertSql + "SELECT SCOPE_IDENTITY();" : qb.InsertSql;
            
        }
        
        public string InsertStatement(bool returnIdentity = false)
        {
            var qb = new QueryBuilder(TableName());
            foreach (var propertyInfo in _entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) || Attribute.IsDefined(propertyInfo, typeof(PKAttribute)))
                {
                    qb.AddIdentityColumn(propertyInfo.Name);
                }

                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotInsertableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute))) continue;
                
                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    qb.Add(field.FieldName, $":{propertyInfo.Name}");
                }
                else
                {
                    qb.Add(propertyInfo.Name, $":{propertyInfo.Name}");
                }
            }

            if (_databaseType == DatabaseType.Postgres)
            {
                return returnIdentity? qb.InsertSql + "SELECT LASTVAL(); " : qb.InsertSql;
            }
            
            return returnIdentity ? qb.InsertSql + "SELECT SCOPE_IDENTITY();" : qb.InsertSql;

        }

        public string UpdateQuery(TEntity entity, string where)
        {
            var qb = new QueryBuilder(TableName(), $" WHERE {where}");
            foreach (var propertyInfo in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotUpdatableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute))) continue;
                
                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    qb.Add(field.FieldName, propertyInfo.GetValue(entity));
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.GetValue(entity));
                }
            }

            return qb.UpdateSql;
        }
        
        public string UpdateStatement(TEntity entity, string where)
        {
            var qb = new QueryBuilder(TableName(), $" WHERE {where}");
            foreach (var propertyInfo in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotUpdatableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute))) continue;
                
                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    qb.Add(field.FieldName, $":{propertyInfo.Name}");
                }
                else
                {
                    qb.Add(propertyInfo.Name, $":{propertyInfo.Name}");
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
            var s = $"[{string.Join("],[", columns.Select(i => i.Replace("[", "")))}]";
            return columns is {Length: > 0}
                ? $"SELECT TOP({top}) {string.Join(",", s)} FROM {TableName()} WHERE {@where};"
                : $"SELECT TOP({top}) * FROM {TableName()} WHERE {@where};";
        }
    }
}