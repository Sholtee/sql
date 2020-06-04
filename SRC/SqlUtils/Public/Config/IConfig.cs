﻿/********************************************************************************
* IConfig.cs                                                                    *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Data;
using System.Reflection;

namespace Solti.Utils.SQL
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
        /// Returns true if the type represents a database entity.
        /// </summary>
        bool IsDatabaseEntity(Type type);
    }
}
