/********************************************************************************
* IDbConnectionExtensions.cs                                                    *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;

using ServiceStack.OrmLite;

namespace Solti.Utils.SQL
{
    using Internals;

    /// <summary>
    /// Defines some handy extensions to the <see cref="IDbConnection"/> interface.
    /// </summary>

    [SuppressMessage("Design", "CA1002:Do not expose generic lists")]
    public static class IDbConnectionOrmLiteExtensions
    {
        /// <summary>
        /// Queries the given <typeparamref name="TView"/>.
        /// </summary>
        public static List<TView> Query<TView>(this IDbConnection connection, Action<IUntypedSqlExpression>? additions = null)
        {
            OrmLiteSqlQuery query = SmartSqlBuilder<TView>.Build(from => new OrmLiteSqlQuery(from));
            additions?.Invoke(query.UnderlyingExpression);
            return query.Run<TView>(connection);      
        }

        /// <summary>
        /// Queries the given <typeparamref name="TView"/>.
        /// </summary>
        public static List<TView> Query<TView>(this IDbConnection connection, ISqlExpression expr) => new OrmLiteSqlQuery
        (
            expr ?? throw new ArgumentNullException(nameof(expr))
        ).Run<TView>(connection);
    }
}
