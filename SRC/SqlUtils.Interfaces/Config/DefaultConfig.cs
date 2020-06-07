/********************************************************************************
* DefaultConfig.cs                                                              *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Solti.Utils.SQL.Interfaces
{
    using DataAnnotations;

    /// <summary>
    /// Default configuration.
    /// </summary>
    public class DefaultConfig: IConfig
    {
        private static readonly Regex FReplacer = new Regex(@"[\x00'""\b\n\r\t\cZ\\%_]");

        /// <summary>
        /// See <see cref="IConfig.Stringify(IDataParameter)"/>
        /// </summary>
        /// <remarks>This is a basic implementation intended for testing purposes only. You have to override it in the derived configuration.</remarks>
        public virtual string Stringify(IDataParameter parameter) 
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (!(parameter.Value is string)) return parameter.Value?.ToString() ?? "NULL";

            string escaped = FReplacer.Replace((string) parameter.Value, match => match.Value switch 
            {
                "\x00"   => "\\0", // terminating null char
                "\b"     => "\\b",
                "\n"     => "\\n",
                "\r"     => "\\r",
                "\t"     => "\\t",
                "\u001A" => "\\Z", // ctr-z
                _ => $"\\{match.Value}"
            });

            return $"\"{escaped}\"";
        }

        /// <summary>
        /// See <see cref="IConfig.IsWrapped(PropertyInfo)"/>.
        /// </summary>
        public virtual bool IsWrapped(PropertyInfo prop)
        {
            if (prop == null) 
                throw new ArgumentNullException(nameof(prop));

            return prop.GetCustomAttribute<WrappedAttribute>() != null;
        }

        /// <summary>
        /// See <see cref="IConfig.IsIgnored(PropertyInfo)"/>.
        /// </summary>
        public virtual bool IsIgnored(PropertyInfo prop) 
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            return prop.GetCustomAttribute<IgnoreAttribute>() != null;
        }

        /// <summary>
        /// See <see cref="IConfig.IsPrimaryKey(PropertyInfo)"/>.
        /// </summary>
        public virtual bool IsPrimaryKey(PropertyInfo prop)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            return prop.GetCustomAttribute<PrimaryKeyAttribute>() != null;
        }

        /// <summary>
        /// See <see cref="IConfig.GetReferencedType(PropertyInfo)"/>.
        /// </summary>
        public virtual Type? GetReferencedType(PropertyInfo prop)
        {
            if (prop == null)
                throw new ArgumentNullException(nameof(prop));

            return prop.GetCustomAttribute<ReferencesAttribute>()?.Type;
        }

        /// <summary>
        /// See <see cref="IConfig.IsDatabaseEntity(Type)"/>.
        /// </summary>
        public virtual bool IsDatabaseEntity(Type type) 
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GetCustomAttribute<DatabaseEntityAttribute>(inherit: false) != null;
        }
    }
}