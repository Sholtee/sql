/********************************************************************************
* OrmLiteSqlQuery.cs                                                            *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using ServiceStack.OrmLite;

namespace Solti.Utils.SQL.Internals
{
    internal class OrmLiteSqlQuery : TypedSqlQuery
    {
        #region Private
        private int FThenBy;
        private readonly IDbConnection FConnection;
        private readonly IOrmLiteDialectProvider FDialectProvider;
        private IUntypedSqlExpression? FSqlExpression;

        //
        // Select() es GroupBy() csak egyszer lehet hivva.
        //

        private readonly List<string>
            FSelectCols = new List<string>(),
            FGroupByCols = new List<string>();
        #endregion

        #region Protected
        protected override void SetBase<TBase>()
        {
            if (FSqlExpression != null)
                throw new InvalidOperationException(); // TODO

            FSqlExpression = FConnection.From<TBase>().GetUntyped();
        }

        protected override void InnerJoin<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> selector) => FSqlExpression!.Join(selector);

        protected override void LeftJoin<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> selector) => FSqlExpression!.LeftJoin(selector);

        protected override void OrderBy<TTable>(Expression<Func<TTable, object>> column)
        {
            if (FThenBy++ == 0) FSqlExpression!.OrderBy(column); else FSqlExpression!.ThenBy(column);
        }

        protected override void OrderByDescending<TTable>(Expression<Func<TTable, object>> column)
        {
            if (FThenBy++ == 0) FSqlExpression!.OrderByDescending(column); else FSqlExpression!.ThenByDescending(column);
        }

        protected override List<TView> Run<TView>()
        {
            const string sep = ", ";

            if (FGroupByCols.Any()) FSqlExpression!.GroupBy(string.Join(sep, FGroupByCols));
            FSqlExpression!.UnsafeSelect(string.Join(sep, FSelectCols));

            return FConnection.Select<TView>(FSqlExpression);
        }
        #endregion

        #region Public
        public IUntypedSqlExpression UnderlyingExpression => FSqlExpression!;

        public OrmLiteSqlQuery(IDbConnection connection)
        {
            FConnection = connection;
            FDialectProvider = connection.GetDialectProvider();
        }

        public OrmLiteSqlQuery(IDbConnection connection, IUntypedSqlExpression sqlExpression): this(connection) 
        {
            FSqlExpression = sqlExpression;
        }

        public override void GroupBy(PropertyInfo column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            FGroupByCols.Add(column.ToSelectString());
        }

        public override void Select(PropertyInfo tableColumn, PropertyInfo viewColumn)
        {
            if (tableColumn == null)
                throw new ArgumentNullException(nameof(tableColumn));

            if (viewColumn == null)
                throw new ArgumentNullException(nameof(viewColumn));

            FSelectCols.Add(Sql.As(tableColumn.ToSelectString(), FDialectProvider.GetQuotedColumnName(viewColumn.Name)));
        }

        private void SelectAggregate(PropertyInfo tableColumn, PropertyInfo viewColumn, Func<string, string> aggregateFn)
        {
            if (tableColumn == null)
                throw new ArgumentNullException(nameof(tableColumn));

            if (viewColumn == null)
                throw new ArgumentNullException(nameof(viewColumn));

            FSelectCols.Add(Sql.As(aggregateFn(tableColumn.ToSelectString()), FDialectProvider.GetQuotedColumnName(viewColumn.Name)));
        }

        public override void SelectAvg(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Avg);

        public override void SelectCount(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Count);

        public override void SelectMax(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Max);

        public override void SelectMin(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Min);
        #endregion
    }
}
