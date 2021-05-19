/********************************************************************************
* OrmLiteSqlQuery.cs                                                            *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using ServiceStack.OrmLite;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;

    internal sealed class OrmLiteSqlQuery : ISqlQuery
    {
        private readonly IOrmLiteDialectProvider FDialectProvider = ServiceStack.OrmLite.OrmLiteConfig.DialectProvider;

        //
        // Select() GroupBy() es OrderBy() csak egyszer lehet hivva.
        //

        private readonly List<string>
            FSelectCols = new(),
            FGroupByCols = new(),
            FOrderByCols = new();

        public void InnerJoin(PropertyInfo left, PropertyInfo right) => UnderlyingExpression.Join(left.ReflectedType, right.ReflectedType, left.ToEqualsExpression(right));

        public void LeftJoin(PropertyInfo left, PropertyInfo right) => UnderlyingExpression.LeftJoin(left.ReflectedType, right.ReflectedType, left.ToEqualsExpression(right));

        public void OrderBy(PropertyInfo column) => FOrderByCols.Add(column.ToSelectString());

        public void OrderByDescending(PropertyInfo column) => FOrderByCols.Add($"{column.ToSelectString()} DESC");

        //
        // Select()-bol nincs nem generikus
        //

        private static readonly MethodInfo FSelect = ((MethodCallExpression) ((Expression<Action<IDbConnection, IUntypedSqlExpression>>) ((conn, expr) => conn.Select<object>(expr, null))).Body)
            .Method
            .GetGenericMethodDefinition();

        public IList Run(IDbConnection conn, Type view)
        {
            const string sep = ", ";

            if (FGroupByCols.Any()) UnderlyingExpression.GroupBy(string.Join(sep, FGroupByCols));
            if (FOrderByCols.Any()) UnderlyingExpression.OrderBy(string.Join(sep, FOrderByCols));
            UnderlyingExpression.UnsafeSelect(string.Join(sep, FSelectCols));

            Func<object?[], object> selectCore = Cache.GetOrAdd(view, () => FSelect.MakeGenericMethod(view).ToStaticDelegate());
            return (IList) selectCore(new object?[] { conn, UnderlyingExpression, null });
        }
     
        public IUntypedSqlExpression UnderlyingExpression { get; }

        //
        // SqlExpression()-bol nincs nem generikus
        //

        private static readonly MethodInfo FSqlExpressionFactory = ((MethodCallExpression) ((Expression<Func<IOrmLiteDialectProvider, IHasUntypedSqlExpression>>) (prov => prov.SqlExpression<object>())).Body)
            .Method
            .GetGenericMethodDefinition();

        public OrmLiteSqlQuery(Type from)
        {
            IHasUntypedSqlExpression hasUntypedSqlExpression = (IHasUntypedSqlExpression) Cache
                .GetOrAdd(from, () => FSqlExpressionFactory.MakeGenericMethod(from).ToInstanceDelegate(), nameof(OrmLiteSqlQuery))
                .Invoke(FDialectProvider, Array.Empty<object?>());
            UnderlyingExpression = hasUntypedSqlExpression.GetUntyped();
        }

        public OrmLiteSqlQuery(ISqlExpression sql) => UnderlyingExpression = sql.GetUntypedSqlExpression();

        public void GroupBy(PropertyInfo column)
        {
            if (column is null)
                throw new ArgumentNullException(nameof(column));

            FGroupByCols.Add(column.ToSelectString());
        }

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

        public void SelectAvg(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Avg);

        public void SelectCount(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Count);

        public void SelectMax(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Max);

        public void SelectMin(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Min);
    }
}
