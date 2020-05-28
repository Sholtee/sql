/********************************************************************************
* DefaultConfig.cs                                                              *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Solti.Utils.SQL
{
    /// <summary>
    /// Default configuration.
    /// </summary>
    public class DefaultConfig: IConfig
    {
        private static readonly Regex FReplacer = new Regex(@"[\x00'""\b\n\r\t\cZ\\%_]");

        /// <summary>
        /// See <see cref="IConfig.Stringify(IDataParameter)"/>
        /// </summary>
        /// <remarks>This is a basic implementation indeed for testing purposes only. You have to override it in the derived configuration.</remarks>
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
    }
}