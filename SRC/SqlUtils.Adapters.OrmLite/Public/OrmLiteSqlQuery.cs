/********************************************************************************
* OrmLiteSqlQuery.cs                                                            *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using ServiceStack.OrmLite;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;

    /// <summary>
    /// Implements the <see cref="ISqlQuery"/> interface.
    /// </summary>
    public sealed class OrmLiteSqlQuery : ISqlQuery
    {
        private readonly IOrmLiteDialectProvider FDialectProvider = ServiceStack.OrmLite.OrmLiteConfig.DialectProvider;

        //
        // Select() GroupBy() es OrderBy() csak egyszer lehet hivva.
        //

        private readonly List<string>
            FSelectCols = new(),
            FGroupByCols = new(),
            FOrderByCols = new();

        /// <inheritdoc/>
        public void InnerJoin(PropertyInfo left, PropertyInfo right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            UnderlyingExpression.Join(left.ReflectedType, right.ReflectedType, left.ToEqualsExpression(right));
        }

        /// <inheritdoc/>
        public void LeftJoin(PropertyInfo left, PropertyInfo right)
        {
            if (left is null)
                throw new ArgumentNullException(nameof(left));

            if (right is null)
                throw new ArgumentNullException(nameof(right));

            UnderlyingExpression.LeftJoin(left.ReflectedType, right.ReflectedType, left.ToEqualsExpression(right));
        }

        /// <inheritdoc/>
        public void OrderBy(PropertyInfo tableColumn)
        {
            if (tableColumn is null)
                throw new ArgumentNullException(nameof(tableColumn));

            FOrderByCols.Add(tableColumn.ToSelectString());
        }

        /// <inheritdoc/>
        public void OrderByDescending(PropertyInfo tableColumn)
        {
            if (tableColumn is null)
                throw new ArgumentNullException(nameof(tableColumn));

            FOrderByCols.Add($"{tableColumn.ToSelectString()} DESC");
        }

        //
        // Select()-bol nincs nem generikus
        //

        private static readonly MethodInfo FSelect = ((MethodCallExpression) ((Expression<Action<IDbConnection, IUntypedSqlExpression>>) ((conn, expr) => conn.Select<object>(string.Empty))).Body)
            .Method
            .GetGenericMethodDefinition();

        /// <inheritdoc/>
        public IList Run(IDbConnection conn, Type view)
        {
            if (conn is null)
                throw new ArgumentNullException(nameof(conn));

            if (view is null)
                throw new ArgumentNullException(nameof(view));

            string sql = ToString();
            Debug.WriteLine(sql);

            Func<object?[], object> selectCore = Cache.GetOrAdd(view, () => FSelect.MakeGenericMethod(view).ToStaticDelegate());

            return (IList) selectCore(new object?[] { conn, sql });
        }

        /// <inheritdoc/>
        public IUntypedSqlExpression UnderlyingExpression { get; }

        //
        // SqlExpression()-bol nincs nem generikus
        //

        private static readonly MethodInfo FSqlExpressionFactory = ((MethodCallExpression) ((Expression<Func<IOrmLiteDialectProvider, IHasUntypedSqlExpression>>) (prov => prov.SqlExpression<object>())).Body)
            .Method
            .GetGenericMethodDefinition();

        /// <summary>
        /// Creates a new <see cref="OrmLiteSqlQuery"/> instance.
        /// </summary>
        public OrmLiteSqlQuery(Type from)
        {
            if (from is null)
                throw new ArgumentNullException(nameof(from));

            IHasUntypedSqlExpression hasUntypedSqlExpression = (IHasUntypedSqlExpression) Cache
                .GetOrAdd(from, () => FSqlExpressionFactory.MakeGenericMethod(from).ToInstanceDelegate(), nameof(OrmLiteSqlQuery))
                .Invoke(FDialectProvider, Array.Empty<object?>());
            UnderlyingExpression = hasUntypedSqlExpression.GetUntyped();
        }

        /// <summary>
        /// Creates a new <see cref="OrmLiteSqlQuery"/> instance.
        /// </summary>
        public OrmLiteSqlQuery(ISqlExpression sql)
        {
            if (sql is null)
                throw new ArgumentNullException(nameof(sql));

            UnderlyingExpression = sql.GetUntypedSqlExpression();
        }

        /// <inheritdoc/>
        public void GroupBy(PropertyInfo tableColumn)
        {
            if (tableColumn is null)
                throw new ArgumentNullException(nameof(tableColumn));

            FGroupByCols.Add(tableColumn.ToSelectString());
        }

        /// <inheritdoc/>
        public void Select(PropertyInfo tableColumn, PropertyInfo viewColumn)
        {
            if (tableColumn is null)
                throw new ArgumentNullException(nameof(tableColumn));

            if (viewColumn is null)
                throw new ArgumentNullException(nameof(viewColumn));

            FSelectCols.Add(Sql.As(tableColumn.ToSelectString(), FDialectProvider.GetQuotedColumnName(viewColumn.Name)));
        }

        private void SelectAggregate(PropertyInfo tableColumn, PropertyInfo viewColumn, Func<string, string> aggregateFn)
        {
            if (tableColumn is null)
                throw new ArgumentNullException(nameof(tableColumn));

            if (viewColumn is null)
                throw new ArgumentNullException(nameof(viewColumn));

            FSelectCols.Add(Sql.As(aggregateFn(tableColumn.ToSelectString()), FDialectProvider.GetQuotedColumnName(viewColumn.Name)));
        }

        /// <inheritdoc/>
        public void SelectAvg(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Avg);

        /// <inheritdoc/>
        public void SelectCount(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Count);

        /// <inheritdoc/>
        public void SelectMax(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Max);

        /// <inheritdoc/>
        public void SelectMin(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Min);

        /// <inheritdoc/>
        public override string ToString()
        {
            const string sep = ", ";

            //
            // GroupBy, OrderBy, es UnsafeSelect() torli a korabbi kivalasztasokat igy nem gond ha tobbszor
            // kerulnek meghivasra.
            //

            if (FGroupByCols.Any()) UnderlyingExpression.GroupBy(string.Join(sep, FGroupByCols));
            if (FOrderByCols.Any()) UnderlyingExpression.OrderBy(string.Join(sep, FOrderByCols));
            if (FSelectCols.Any()) UnderlyingExpression.UnsafeSelect(string.Join(sep, FSelectCols));

            return FDialectProvider.MergeParamsIntoSql(UnderlyingExpression.ToSelectStatement(), UnderlyingExpression.Params);
        }
    }
}
