using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ClearDashboard.DataAccessLayer.Data.Extensions
{
    public static class DbCommandExtensions
    {
        public const string DISCRIMINATOR_COLUMN_NAME = "Discriminator";

        public static string CreateAddParameter(this DbCommand dbCommand, IProperty property, int nameIndex = -1)
        {
            var pName = dbCommand.ToParameterName(property.Name, nameIndex);

            var parameter = dbCommand.CreateParameter();
            parameter.ParameterName = pName;

            if (property.Name == DISCRIMINATOR_COLUMN_NAME && nameIndex < 0)
            {
                parameter.SetAsDiscriminator();
            }
            else
            {
                parameter.SetDbType(property.ClrType);
            }

            dbCommand.Parameters.Add(parameter);

            return pName;
        }

        public static string ToParameterName(this DbCommand dbCommand, string name, int nameIndex = -1)
        {
            return nameIndex < 0
                ? $"@{ToDbCommandParameterName(name)}"
                : $"@{ToDbCommandParameterName(name)}{nameIndex}";
        }

        private static string ToDbCommandParameterName(string parameterName)
        {
            return parameterName.Replace('.', '_');
        }

        public static void SetDbType(this DbParameter dbParameter, Type clrType)
        {
            if (dbParameter is Npgsql.NpgsqlParameter parameter)
            {
                parameter.NpgsqlDbType = clrType switch
                {
                    Type _ when clrType == typeof(Guid) => NpgsqlTypes.NpgsqlDbType.Uuid,
                    Type _ when clrType == typeof(Guid?) => NpgsqlTypes.NpgsqlDbType.Uuid,
                    Type _ when clrType == typeof(int) => NpgsqlTypes.NpgsqlDbType.Integer,
                    Type _ when clrType == typeof(double) => NpgsqlTypes.NpgsqlDbType.Double,
                    Type _ when clrType == typeof(bool) => NpgsqlTypes.NpgsqlDbType.Boolean,
                    Type _ when clrType == typeof(string) => NpgsqlTypes.NpgsqlDbType.Text,
                    Type _ when clrType == typeof(DateTimeOffset) => NpgsqlTypes.NpgsqlDbType.TimestampTz,
                    Type _ when clrType == typeof(DateTimeOffset?) => NpgsqlTypes.NpgsqlDbType.TimestampTz,
                    Type _ when clrType.IsAssignableTo(typeof(System.Enum)) => NpgsqlTypes.NpgsqlDbType.Integer,
                    Type _ when IsAssignableToGenericType(clrType, typeof(Dictionary<,>)) => NpgsqlTypes.NpgsqlDbType.Jsonb,
                    _ => throw new NotSupportedException($"No DbType configured for model type {clrType.FullName}")
                };
            }
            
            // Otherwise don't do anything - Sqlite doesn't seem to need it
        }

        public static (string Name, Type ModelType) GetDiscriminatorNameType(this Type modelType)
        {
            return (DISCRIMINATOR_COLUMN_NAME, typeof(string));
        }

        public static void SetAsDiscriminator(this DbParameter dbParameter)
        {
            if (dbParameter is Npgsql.NpgsqlParameter parameter)
            {
                parameter.NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Varchar;
            }
            
            // Otherwise don't do anything - Sqlite doesn't seem to need it
        }

        public static IEntityType ToEntityType(this IModel metadataModel, Type clrType)
        {
            return metadataModel.FindEntityType(clrType)
                ?? throw new Exception($"Unable to find IEntityType for CLR type {clrType.Name}");
        }

        public static string ToTableName(this IModel metadataModel, Type clrType)
        {
            return metadataModel.ToEntityType(clrType).ToTableName();
        }

        public static string ToTableName(this IEntityType entityType)
        {
            return entityType.GetTableName() 
                ?? throw new Exception($"No table name associated with IEntityType {entityType.DisplayName()}");
        }

        public static IEnumerable<IProperty> ToProperties(this IEntityType entityType, IReadOnlyList<string> propertyNames)
        {
            return entityType.FindProperties(propertyNames)
                ?? throw new Exception($"One or more property names from {string.Join('|', propertyNames)} not found on entity type {entityType.Name}");
        }

        public static IProperty ToProperty(this IEntityType entityType, string propertyName)
        {
            return entityType.FindProperty(propertyName)
                ?? throw new Exception($"Property name {propertyName} not found on entity type {entityType.Name}");
        }

        public static IEnumerable<IProperty> ToStoredProperties(this IEntityType entityType)
        {
            return entityType.GetProperties().Where(p => p.GetIsStored() == true);
        }

        public static IEnumerable<PropertyInfo> GetModelReadWriteProperties(this Type modelType)
        { 
            return modelType.GetProperties(
                    System.Reflection.BindingFlags.Instance | 
                    System.Reflection.BindingFlags.Public)
                .Where(p => p.CanRead)
                .Where(p => p.CanWrite)
                .Where(p => !p.IsDefined(typeof(NotMappedAttribute), true))
                .Where(p => p.GetIndexParameters().Length == 0)
                .ToList();
        }

        // TODO:  this is in two places - consolidate!
        public static bool IsAssignableToGenericType(Type type, Type genericTypeDefintion)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == genericTypeDefintion)
            {
                return true;
            }

            if (type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericTypeDefintion))
            {
                return true;
            }

            Type? baseType = type.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, genericTypeDefintion);
        }
    }
}