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
        string Stringify(IDataParameter parameter);

        /// <summary>
        /// Returns true if the property represents a list and should be used in relation mapping.
        /// </summary>
        bool IsWrapped(PropertyInfo prop);

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
        bool IsDatabaseEntity(Type type);

        /// <summary>
        /// Gets the data table referred by the foreign key.
        /// </summary>
        Type? GetReferencedType(PropertyInfo prop);
    }
}
