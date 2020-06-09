/********************************************************************************
* IDbConnectionExtensions.cs                                                    *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;

using ServiceStack.OrmLite;

namespace Solti.Utils.SQL
{
    /// <summary>
    /// Defines some handy extensions to the <see cref="IDbConnection"/> interface.
    /// </summary>
    public static class IDbConnectionExtensions
    {
        private static OrmLiteSqlQuery CreateQuery<TView>(this IDbConnection connection) 
        {
            var query = new OrmLiteSqlQuery(connection ?? throw new ArgumentNullException(nameof(connection)));

            SmartSqlBuilder<TView>.Build(query);

            return query;
        }

        /// <summary>
        /// Creates the query expression from the given <typeparamref name="TView"/>.
        /// </summary>
        public static IUntypedSqlExpression FromView<TView>(this IDbConnection connection) => connection.CreateQuery<TView>().UnderlyingExpression;

        /// <summary>
        /// Queries the given <typeparamref name="TView"/>.
        /// </summary>
        public static List<TView> Query<TView>(this IDbConnection connection, Action<IUntypedSqlExpression>? additions = null)
        {
            var query = connection.CreateQuery<TView>();
            additions?.Invoke(query.UnderlyingExpression);
            return query.Run<TView>();      
        }
    }
}
