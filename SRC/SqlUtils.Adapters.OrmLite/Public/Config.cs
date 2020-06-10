﻿/********************************************************************************
* Config.cs                                                                     *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Data;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    using Internals;

    /// <summary>
    /// Library specific configuration.
    /// </summary>
    public sealed class OrmLiteConfig: DefaultConfig
    {
        /// <summary>
        /// See <see cref="IConfig.GetReferencedType(PropertyInfo)"/>.
        /// </summary>
        public override Type? GetReferencedType(PropertyInfo prop)
        {
            if (prop == null) throw new ArgumentNullException(nameof(prop));

            return prop.GetFieldDefinition()?.ForeignKey?.ReferenceType;
        }

        /// <summary>
        /// See <see cref="IConfig.IsPrimaryKey(PropertyInfo)"/>.
        /// </summary>
        public override bool IsPrimaryKey(PropertyInfo prop)
        {
            if (prop == null) throw new ArgumentNullException(nameof(prop));

            return !IsIgnored(prop) && prop.GetFieldDefinition().IsPrimaryKey;
        }

        /// <summary>
        /// See <see cref="IConfig.IsIgnored(PropertyInfo)"/>.
        /// </summary>
        public override bool IsIgnored(PropertyInfo prop)
        {
            if (prop == null) throw new ArgumentNullException(nameof(prop));

            return prop.GetFieldDefinition() == null;
        }

        /// <summary>
        /// See <see cref="IConfig.Stringify(IDataParameter)"/>.
        /// </summary>
        public override string Stringify(IDataParameter parameter)
        {
            if (parameter == null) throw new ArgumentNullException(nameof(parameter));

            object value = parameter.Value;

            return value != null
                ? ServiceStack.OrmLite.OrmLiteConfig.DialectProvider.GetQuotedValue(value, value.GetType())
                : "NULL";
        }
    }
}