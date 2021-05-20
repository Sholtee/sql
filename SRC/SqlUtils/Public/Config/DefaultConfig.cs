/********************************************************************************
* DefaultConfig.cs                                                              *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Solti.Utils.SQL
{
    using Interfaces;
    using Interfaces.DataAnnotations;

    /// <summary>
    /// Default configuration.
    /// </summary>
    public class DefaultConfig: IConfig
    {
        private static readonly Regex FReplacer = new Regex(@"[\x00'""\b\n\r\t\cZ\\%_]", RegexOptions.Compiled);

        /// <summary>
        /// See <see cref="IConfig.Stringify(IDbDataParameter)"/>
        /// </summary>
        /// <remarks>This is a basic implementation intended for testing purposes only. You have to override it in the derived configuration.</remarks>
        public virtual string Stringify(IDbDataParameter parameter) 
        {
            if (parameter is null)
                throw new ArgumentNullException(nameof(parameter));

            if (parameter.Value is not string)
                return parameter.Value?.ToString() ?? "NULL";

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

        private static readonly Regex FFormatter = new(@"\?|@\w+|{\d+}", RegexOptions.Compiled);

        /// <summary>
        /// Formats the given SQL template. Templates may contain positional (?), named (@Name), or indexed ({0}) placeholders. 
        /// </summary>
        [SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
        public virtual string SqlFormat(string sql, params IDbDataParameter[] paramz)
        {
            if (sql is null)
                throw new ArgumentNullException(nameof(sql));

            if (paramz is null)
                throw new ArgumentNullException(nameof(paramz));

            int index = 0;

            return FFormatter.Replace(sql, match =>
            {
                string placeholder = match.Value;

                IDbDataParameter matchingParameter;

                switch (placeholder[0])
                {
                    case '?':
                        if (index == paramz.Length) throw new IndexOutOfRangeException();
                        matchingParameter = paramz[index++];
                        break;
                    case '@':
                        matchingParameter = paramz.SingleOrDefault(p =>
                        {
                            string name = p.ParameterName;
                            if (!name.StartsWith("@", StringComparison.Ordinal)) name = $"@{name}";
                            return placeholder == name;
                        });
                        if (matchingParameter is null) throw new KeyNotFoundException();
                        break;
                    case '{':
                        uint i = uint.Parse(placeholder.Trim('{', '}'), null);
                        if (i >= paramz.Length) throw new IndexOutOfRangeException();
                        matchingParameter = paramz[i];
                        break;
                    default:
                        throw new NotSupportedException();
                }

                return Stringify(matchingParameter);
            });
        }

        /// <summary>
        /// See <see cref="IConfig.IsIgnored(PropertyInfo)"/>.
        /// </summary>
        public virtual bool IsIgnored(PropertyInfo prop) 
        {
            if (prop is null)
                throw new ArgumentNullException(nameof(prop));

            return prop.GetCustomAttribute<IgnoreAttribute>() is not null;
        }

        /// <summary>
        /// See <see cref="IConfig.IsPrimaryKey(PropertyInfo)"/>.
        /// </summary>
        public virtual bool IsPrimaryKey(PropertyInfo prop)
        {
            if (prop is null)
                throw new ArgumentNullException(nameof(prop));

            return prop.GetCustomAttribute<PrimaryKeyAttribute>() is not null;
        }

        /// <summary>
        /// See <see cref="IConfig.GetReferencedType(PropertyInfo)"/>.
        /// </summary>
        public virtual Type? GetReferencedType(PropertyInfo prop)
        {
            if (prop is null)
                throw new ArgumentNullException(nameof(prop));

            return prop.GetCustomAttribute<ReferencesAttribute>()?.Type;
        }

        /// <summary>
        /// See <see cref="IConfig.IsDataTable(Type)"/>.
        /// </summary>
        public virtual bool IsDataTable(Type type) 
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.GetCustomAttribute<DatabaseEntityAttribute>(inherit: false) is not null;
        }

        /// <summary>
        /// See <see cref="IConfig.CreateQuery(Type)"/>.
        /// </summary>
        public virtual ISqlQuery CreateQuery(Type from) => throw new NotImplementedException();
    }
}