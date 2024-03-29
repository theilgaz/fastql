﻿using System;
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
                var attribute = type.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(TableAttribute));
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

        public string InsertReturnObjectQuery()
        {
            FastQueryBuilder qb = new FastQueryBuilder(TableName());
            TEntity entity = _entity;
            PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)))
                {
                    qb.AddIdentityColumn(propertyInfo.Name);
                }

                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotInsertableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(CustomFieldAttribute)))
                {
                    continue;
                }

                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    FieldAttribute field =
                        (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    switch (field.FieldType)
                    {
                        case FieldType.Jsonb:
                            qb.Add(field.FieldName, propertyInfo.Name + "::jsonb", propertyInfo.GetValue(_entity));
                            break;
                        case FieldType.Timestamp:
                            qb.Add(field.FieldName, propertyInfo.Name + "::timestamp", propertyInfo.GetValue(_entity));
                            break;
                        case FieldType.Time:
                            qb.Add(field.FieldName, propertyInfo.Name + "::time", propertyInfo.GetValue(_entity));
                            break;
                        case FieldType.Date:
                            qb.Add(field.FieldName, propertyInfo.Name + "::date", propertyInfo.GetValue(_entity));
                            break;
                        default:
                            qb.Add(field.FieldName, propertyInfo.Name, propertyInfo.GetValue(_entity));
                            break;
                    }
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name, propertyInfo.GetValue(_entity));
                }
            }

            if (_databaseType == DatabaseType.Postgres)
            {
                return qb.InsertSql + $" RETURNING {qb.ReturnStatement}; ";
            }

            return qb.InsertReturnObjectSql;
        }

        public string InsertQuery(bool returnIdentity = false)
        {
            var qb = new FastQueryBuilder(TableName());
            foreach (var propertyInfo in _entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)))
                {
                    qb.AddIdentityColumn(propertyInfo.Name);
                }

                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotInsertableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(CustomFieldAttribute))) continue;

                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    switch (field.FieldType)
                    {
                        case FieldType.Jsonb:
                            qb.Add(field.FieldName, propertyInfo.Name + "::jsonb", propertyInfo.GetValue(_entity));
                            break;
                        case FieldType.Timestamp:
                            qb.Add(field.FieldName, propertyInfo.Name + "::timestamp", propertyInfo.GetValue(_entity));
                            break;
                        case FieldType.Time:
                            qb.Add(field.FieldName, propertyInfo.Name + "::time", propertyInfo.GetValue(_entity));
                            break;
                        case FieldType.Date:
                            qb.Add(field.FieldName, propertyInfo.Name + "::date", propertyInfo.GetValue(_entity));
                            break;
                        default:
                            qb.Add(field.FieldName, propertyInfo.Name, propertyInfo.GetValue(_entity));
                            break;
                    }
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name, propertyInfo.GetValue(_entity));
                }
            }

            if (_databaseType == DatabaseType.Postgres)
            {
                return returnIdentity ? qb.InsertSql + " RETURNING ID; " : qb.InsertSql;
            }

            return returnIdentity ? qb.InsertSql + "; SELECT SCOPE_IDENTITY();" : qb.InsertSql;
        }

        public string InsertStatement(bool returnIdentity = false)
        {
            var qb = new FastQueryBuilder(TableName());
            foreach (var propertyInfo in _entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)))
                {
                    qb.AddIdentityColumn(propertyInfo.Name);
                }

                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotInsertableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(CustomFieldAttribute))) continue;

                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    qb.Add(field.FieldName, propertyInfo.Name, $":{propertyInfo.Name}");
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name, $":{propertyInfo.Name}");
                }
            }

            if (_databaseType == DatabaseType.Postgres)
            {
                return returnIdentity ? qb.InsertSql + " RETURNING ID; " : qb.InsertSql;
            }

            return returnIdentity ? qb.InsertSql + "; SELECT SCOPE_IDENTITY();" : qb.InsertSql;
        }

        public string UpdateQuery(TEntity entity, string where)
        {
            var qb = new FastQueryBuilder(TableName(), $" WHERE {where}");
            foreach (var propertyInfo in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotUpdatableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(CustomFieldAttribute))) continue;

                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    switch (field.FieldType)
                    {
                        case FieldType.Jsonb:
                            qb.Add(field.FieldName, propertyInfo.Name + "::jsonb", propertyInfo.GetValue(entity));
                            break;
                        case FieldType.Timestamp:
                            qb.Add(field.FieldName, propertyInfo.Name + "::timestamp", propertyInfo.GetValue(entity));
                            break;
                        case FieldType.Time:
                            qb.Add(field.FieldName, propertyInfo.Name + "::time", propertyInfo.GetValue(entity));
                            break;
                        case FieldType.Date:
                            qb.Add(field.FieldName, propertyInfo.Name + "::date", propertyInfo.GetValue(entity));
                            break;
                        default:
                            qb.Add(field.FieldName, propertyInfo.Name, propertyInfo.GetValue(entity));
                            break;
                    }
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name, propertyInfo.GetValue(entity));
                }
            }

            return qb.UpdateSql;
        }

        public string UpdateStatement(TEntity entity, string where)
        {
            var qb = new FastQueryBuilder(TableName(), $" WHERE {where}");
            foreach (var propertyInfo in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotUpdatableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(CustomFieldAttribute))) continue;

                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    qb.Add(field.FieldName, propertyInfo.Name, $":{propertyInfo.Name}");
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name, $":{propertyInfo.Name}");
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
            FastQueryBuilder qb = new FastQueryBuilder(TableName(), " WHERE " + where);
            TEntity entity = _entity;
            PropertyInfo[] properties = entity.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (Attribute.IsDefined(propertyInfo, typeof(CustomFieldAttribute))) continue;
                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    FieldAttribute fieldAttribute = (FieldAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    qb.Add(fieldAttribute.FieldName, propertyInfo.Name, "");
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name, "");
                }
            }
            return qb.SelectSql;
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