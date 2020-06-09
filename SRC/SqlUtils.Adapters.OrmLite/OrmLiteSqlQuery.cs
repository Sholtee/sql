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

namespace Solti.Utils.SQL
{
    using Interfaces;

    /// <summary>
    /// The OrmLite specific implementation of the <see cref="ISqlQuery"/> interface.
    /// </summary>
    public class OrmLiteSqlQuery<TBaseTable> : TypedSqlQuery
    {
        #region Private
        private int FThenBy;
        private readonly IDbConnection FConnection;
        private readonly IOrmLiteDialectProvider FDialectProvider;
        private readonly SqlExpression<TBaseTable> FSqlExpression;

        //
        // Select() es GroupBy() csak egyszer lehet hivva.
        //

        private readonly List<string>
            FSelectCols = new List<string>(),
            FGroupByCols = new List<string>();
        #endregion

        #region Protected
        /// <summary>
        /// See <see cref="ISqlQuery.Finalize(Type)"/>.
        /// </summary>
        protected override void Finalize<TView>() {}

        /// <summary>
        /// See <see cref="ISqlQuery.InnerJoin(PropertyInfo, PropertyInfo)"/>.
        /// </summary>
        protected override void InnerJoin<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> selector) => FSqlExpression.Join(selector);

        /// <summary>
        /// See <see cref="ISqlQuery.LeftJoin(PropertyInfo, PropertyInfo)"/>.
        /// </summary>
        protected override void LeftJoin<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> selector) => FSqlExpression.LeftJoin(selector);

        /// <summary>
        /// See <see cref="ISqlQuery.OrderBy(PropertyInfo)"/>.
        /// </summary>
        protected override void OrderBy<TTable>(Expression<Func<TTable, object>> column)
        {
            if (FThenBy++ == 0) FSqlExpression.OrderBy(column); else FSqlExpression.ThenBy(column);
        }

        /// <summary>
        /// See <see cref="ISqlQuery.OrderByDescending(PropertyInfo)"/>.
        /// </summary>
        protected override void OrderByDescending<TTable>(Expression<Func<TTable, object>> column)
        {
            if (FThenBy++ == 0) FSqlExpression.OrderByDescending(column); else FSqlExpression.ThenByDescending(column);
        }

        /// <summary>
        /// See <see cref="ISqlQuery.Run(Type)"/>.
        /// </summary>
        protected override List<TView> Run<TView>()
        {
            const string sep = ", ";

            if (FGroupByCols.Any()) FSqlExpression.GroupBy(string.Join(sep, FGroupByCols));
            FSqlExpression.UnsafeSelect(string.Join(sep, FSelectCols), distinct: false);

            return FConnection.Select<TView>(FSqlExpression);
        }
        #endregion

        #region Public
        /// <summary>
        /// Creates a new <see cref="OrmLiteSqlQuery{TBaseTable}"/> instance.
        /// </summary>
        public OrmLiteSqlQuery(IDbConnection connection, SqlExpression<TBaseTable> sqlExpression)
        {
            FSqlExpression = sqlExpression ?? throw new ArgumentNullException(nameof(sqlExpression));
            FConnection = connection ?? throw new ArgumentNullException(nameof(connection));
            FDialectProvider = FConnection.GetDialectProvider();
        }

        /// <summary>
        /// See <see cref="ISqlQuery.GroupBy(PropertyInfo)"/>.
        /// </summary>
        public override void GroupBy(PropertyInfo column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            FGroupByCols.Add(column.ToSelectString());
        }

        /// <summary>
        /// See <see cref="ISqlQuery.Select(PropertyInfo, PropertyInfo)"/>.
        /// </summary>
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

        /// <summary>
        /// See <see cref="ISqlQuery.SelectAvg(PropertyInfo, PropertyInfo)"/>.
        /// </summary>
        public override void SelectAvg(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Avg);

        /// <summary>
        /// See <see cref="ISqlQuery.SelectCount(PropertyInfo, PropertyInfo)"/>.
        /// </summary>
        public override void SelectCount(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Count);

        /// <summary>
        /// See <see cref="ISqlQuery.SelectMax(PropertyInfo, PropertyInfo)"/>.
        /// </summary>
        public override void SelectMax(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Max);

        /// <summary>
        /// See <see cref="ISqlQuery.SelectMin(PropertyInfo, PropertyInfo)"/>.
        /// </summary>
        public override void SelectMin(PropertyInfo tableColumn, PropertyInfo viewColumn) => SelectAggregate(tableColumn, viewColumn, Sql.Min);
        #endregion
    }
}
