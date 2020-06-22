/********************************************************************************
* IDbConnectionExtensions.cs                                                    *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using ServiceStack.OrmLite;

namespace Solti.Utils.SQL
{
    using Internals; 

    /// <summary>
    /// Defines some handy extensions to the <see cref="IDbConnection"/> interface.
    /// </summary>
    public static class IDbConnectionOrmLiteExtensions
    {
        /// <summary>
        /// Queries the given <typeparamref name="TView"/>.
        /// </summary>
        public static List<TView> Query<TView>(this IDbConnection connection, Action<IUntypedSqlExpression>? additions = null)
        {
            var query = new OrmLiteSqlQuery(connection ?? throw new ArgumentNullException(nameof(connection)));
            SmartSqlBuilder<TView>.Build(query);
            additions?.Invoke(query.UnderlyingExpression);
            return query.Run<TView>();      
        }

        /// <summary>
        /// Queries the given <typeparamref name="TView"/>.
        /// </summary>
        public static List<TView> Query<TView>(this IDbConnection connection, IUntypedSqlExpression expr) => new OrmLiteSqlQuery
        (
            connection ?? throw new ArgumentNullException(nameof(connection)), 
            expr ?? throw new ArgumentNullException(nameof(expr))
        ).Run<TView>();

        /// <summary>
        /// Queries the given <typeparamref name="TView"/>.
        /// </summary>
        public static List<TView> Query<TView>(this IDbConnection connection, string sql) => new StringBasedOrmLiteSqlQuery
        (
            connection ?? throw new ArgumentNullException(nameof(connection)),
            sql ?? throw new ArgumentNullException(nameof(sql))
        ).Run<TView>();

        /// <summary>
        /// Creates the data tables (if they don't exist).
        /// </summary>
        public static void CreateSchema(this IDbConnection connection)
        {
            using (IBulkedDbConnection conn = connection.CreateBulkedDbConnection())
            {
                connection.CreateTableIfNotExists(Config.KnownTables.ToArray());
                conn.Flush();
            }
        }
    }
}
