/********************************************************************************
* IConfig.cs                                                                    *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Data;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Defines the abstract configuration related to this library.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Stringifies the given parameter.
        /// </summary>
        string Stringify(IDbDataParameter parameter);

        /// <summary>
        /// Merges the given parameters into the query string.
        /// </summary>
        /// <returns></returns>
        string SqlFormat(string sql, params IDbDataParameter[] paramz);

        /// <summary>
        /// Returns true if the property should be ignored.
        /// </summary>
        bool IsIgnored(PropertyInfo prop);

        /// <summary>
        /// Returns true if the property represents a PK column.
        /// </summary>
        bool IsPrimaryKey(PropertyInfo prop);

        /// <summary>
        /// Returns true if the type represents a database entity.
        /// </summary>
        bool IsDataTable(Type type);

        /// <summary>
        /// Gets the data table referred by the foreign key.
        /// </summary>
        Type? GetReferencedType(PropertyInfo prop);

        /// <summary>
        /// Creates a new <see cref="ISqlQuery"/> instance.
        /// </summary>
        ISqlQuery CreateQuery(Type from);
    }
}
