/********************************************************************************
* OrmLiteConfig.cs                                                              *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    using Internals;

    /// <summary>
    /// OrmLite specific configuration.
    /// </summary>
    public sealed class OrmLiteConfig: DefaultConfig
    {
        /// <inheritdoc/>
        public override ISqlQuery CreateQuery(Type from) => new OrmLiteSqlQuery(from);

        /// <inheritdoc/>
        public override Type? GetReferencedType(PropertyInfo prop)
        {
            if (prop is null)
                throw new ArgumentNullException(nameof(prop));

            return prop.GetFieldDefinition()?.ForeignKey?.ReferenceType;
        }

        /// <inheritdoc/>
        public override bool IsPrimaryKey(PropertyInfo prop)
        {
            if (prop is null) 
                throw new ArgumentNullException(nameof(prop));

            return !IsIgnored(prop) && prop.GetFieldDefinition().IsPrimaryKey;
        }

        /// <inheritdoc/>
        public override bool IsIgnored(PropertyInfo prop)
        {
            if (prop is null) 
                throw new ArgumentNullException(nameof(prop));

            return prop.GetFieldDefinition() is null;
        }

        /// <inheritdoc/>
        public override string Stringify(IDbDataParameter parameter)
        {
            if (parameter is null) 
                throw new ArgumentNullException(nameof(parameter));

            object? value = parameter.Value;

            return value is not null
                ? ServiceStack.OrmLite.OrmLiteConfig.DialectProvider.GetQuotedValue(value, value.GetType())
                : "NULL";
        }

        /// <inheritdoc/>
        public override string SqlFormat(string sql, params IDbDataParameter[] paramz)
        {
            if (sql is null)
                throw new ArgumentNullException(nameof(sql));

            if (paramz is null)
                throw new ArgumentNullException(nameof(paramz));

            return ServiceStack.OrmLite.OrmLiteConfig.DialectProvider.MergeParamsIntoSql(sql, paramz.Select(para =>
            {
                //
                // MergeParamsIntoSql() baszik rendesen lekezeni a DBNull-t
                //

                if (para.Value == DBNull.Value)
                    para.Value = null;

                return para;
            }));
        }
    }
}
