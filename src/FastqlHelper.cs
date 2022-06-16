using System;
using System.Linq;
using System.Reflection;
using Fastql.Attributes;
using Fastql.Utilities;

namespace Fastql
{
    public static class FastqlHelper<TEntity>
    {
        private static DatabaseType _databaseType = DatabaseType.SqlServer;
        
        public static DatabaseType DatabaseType
        {
            get { return _databaseType; }
            set { _databaseType = value; }
        }
        
        public static DatabaseType SetDatabaseType(DatabaseType databaseType)
        {
            _databaseType = databaseType;
            return _databaseType;
        }
        
        public static DatabaseType GetDatabaseType()
        {
            return _databaseType;
        }
        
        
        static string TableName()
        {
            var type = Activator.CreateInstance(typeof(TEntity)).GetType();
            if (type.CustomAttributes.Any())
            {
                var attribute = type.CustomAttributes.FirstOrDefault(x=>x.AttributeType == typeof(TableAttribute));
                if (attribute.AttributeType.Name == "TableAttribute")
                {
                    var table = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute));

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

        public static string InsertQuery(bool returnIdentity = false)
        {
            var instance = (TEntity)Activator.CreateInstance(typeof(TEntity));
            var qb = new FastQueryBuilder(TableName());
            foreach (var propertyInfo in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
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
                    qb.Add(field.FieldName, propertyInfo.Name, propertyInfo.GetValue(instance));
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name, propertyInfo.GetValue(instance));
                }
            }
            
            if (_databaseType == DatabaseType.Postgres)
            {
                return returnIdentity? qb.InsertSql + "SELECT LASTVAL(); " : qb.InsertSql;
            }
            
            return returnIdentity ? qb.InsertSql + "SELECT SCOPE_IDENTITY();" : qb.InsertSql;

        }

        public static string InsertStatement(bool returnIdentity = false)
        {
            var instance = (TEntity)Activator.CreateInstance(typeof(TEntity));
            var qb = new FastQueryBuilder(TableName());
            foreach (var propertyInfo in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
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
                    qb.Add(field.FieldName, propertyInfo.Name,$":{propertyInfo.Name}");
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name,$":{propertyInfo.Name}");
                }
            }

            if (_databaseType == DatabaseType.Postgres)
            {
                return returnIdentity? qb.InsertSql + "SELECT LASTVAL(); " : qb.InsertSql;
            }
            
            return returnIdentity ? qb.InsertSql + "SELECT SCOPE_IDENTITY();" : qb.InsertSql;

        }
        
        public static string SelectQuery(string where)
        {
            return $"SELECT * FROM {TableName()} WHERE {@where};";
        }

        public static string SelectQuery(string[] columns, string where, int top = 1000)
        {
            var s = $"[{string.Join("],[", columns.Select(i => i.Replace("[", "")))}]";
            return columns is { Length: > 0 }
                ? $"SELECT TOP({top}) {string.Join(",", s)} FROM {TableName()} WHERE {@where};"
                : $"SELECT TOP({top}) * FROM {TableName()} WHERE {@where};";
        }

        public static string UpdateQuery(TEntity entity, string where)
        {
            var qb = new FastQueryBuilder(TableName(), $" WHERE {where}");
            foreach (var propertyInfo in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotUpdatableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute))) continue;
                
                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    qb.Add(field.FieldName, propertyInfo.Name, propertyInfo.GetValue(entity));
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name, propertyInfo.GetValue(entity));
                }
            }

            return qb.UpdateSql;
        }
        
        public static string UpdateStatement(TEntity entity, string where)
        {
            var qb = new FastQueryBuilder(TableName(), $" WHERE {where}");
            foreach (var propertyInfo in entity.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(PKAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(IsNotUpdatableAttribute)) ||
                    Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute))) continue;
                
                if (Attribute.IsDefined(propertyInfo, typeof(FieldAttribute)))
                {
                    var field = (FieldAttribute) Attribute.GetCustomAttribute(propertyInfo, typeof(FieldAttribute));
                    qb.Add(field.FieldName, propertyInfo.Name, $":{propertyInfo.Name}");
                }
                else
                {
                    qb.Add(propertyInfo.Name, propertyInfo.Name,$":{propertyInfo.Name}");
                }
            }

            return qb.UpdateSql;
        }

        public static string DeleteQuery(string where)
        {
            return $"DELETE FROM {TableName()} WHERE {where};";
        }
    }
}