/********************************************************************************
* TypedSqlQuery.cs                                                              *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal class TypedSqlQuery : ISqlQuery
    {
        private static MethodInfo GetGenericMethod(Expression<Action<TypedSqlQuery>> expr) => ((MethodCallExpression) expr.Body).Method.GetGenericMethodDefinition();

        private static readonly MethodInfo FSetBase = GetGenericMethod(self => self.SetBase<object>());

        public virtual void SetBase(Type view) => FSetBase.MakeGenericMethod(view ?? throw new ArgumentNullException(nameof(view))).Call(this);

        protected virtual void SetBase<TView>() => throw new NotImplementedException();

        private static readonly MethodInfo FGroupBy = GetGenericMethod(self => self.GoupBy<object>(null!));

        public virtual void GroupBy(PropertyInfo column)
        {
            if (column == null) throw new ArgumentNullException(nameof(column));

            FGroupBy.MakeGenericMethod(column.ReflectedType).Call
            (
                this,
                column.ToSelectExpression()
            );
        }

        protected virtual void GoupBy<TTable>(Expression<Func<TTable, object>> column) => throw new NotImplementedException();

        private static readonly MethodInfo FInnerJoin = GetGenericMethod(self => self.InnerJoin<object, object>(null!));

        public virtual void InnerJoin(PropertyInfo left, PropertyInfo right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            FInnerJoin.MakeGenericMethod(left.ReflectedType, right.ReflectedType).Call
            (
                this,
                left.ToEqualsExpression(right)
            );
        }

        protected virtual void InnerJoin<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> selector) => throw new NotImplementedException();

        private static readonly MethodInfo FLeftJoin = GetGenericMethod(self => self.LeftJoin<object, object>(null!));

        public virtual void LeftJoin(PropertyInfo left, PropertyInfo right)
        {
            if (left == null)
                throw new ArgumentNullException(nameof(left));

            if (right == null)
                throw new ArgumentNullException(nameof(right));

            FLeftJoin.MakeGenericMethod(left.ReflectedType, right.ReflectedType).Call
            (
                this,
                left.ToEqualsExpression(right)
            );
        }

        protected virtual void LeftJoin<TTable1, TTable2>(Expression<Func<TTable1, TTable2, bool>> selector) => throw new NotImplementedException();

        private static readonly MethodInfo FOrderBy = GetGenericMethod(self => self.OrderBy<object>(null!));

        public virtual void OrderBy(PropertyInfo column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            FOrderBy.MakeGenericMethod(column.ReflectedType).Call
            (
                this,
                column.ToSelectExpression()
            );
        }

        protected virtual void OrderBy<TTable>(Expression<Func<TTable, object>> column) => throw new NotImplementedException();

        private static readonly MethodInfo FOrderByDescending = GetGenericMethod(self => self.OrderByDescending<object>(null!));

        public virtual void OrderByDescending(PropertyInfo column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            FOrderByDescending.MakeGenericMethod(column.ReflectedType).Call
            (
                this,
                column.ToSelectExpression()
            );
        }

        protected virtual void OrderByDescending<TTable>(Expression<Func<TTable, object>> column) => throw new NotImplementedException();

        private static readonly MethodInfo FRun = GetGenericMethod(self => self.Run<object>());

        public virtual IList Run(Type view) => (IList) FRun.MakeGenericMethod(view ?? throw new ArgumentNullException(nameof(view))).Call(this);

        protected virtual List<TView> Run<TView>() => throw new NotImplementedException();

        private static readonly MethodInfo FSelect = GetGenericMethod(self => self.Select<object, object>(null!, null!));

        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
        public virtual void Select(PropertyInfo tableColumn, PropertyInfo viewColumn)
        {
            if (tableColumn == null)
                throw new ArgumentNullException(nameof(tableColumn));

            if (viewColumn == null)
                throw new ArgumentNullException(nameof(viewColumn));

            FSelect.MakeGenericMethod(tableColumn.ReflectedType, viewColumn.ReflectedType).Call
            (
                this,
                tableColumn.ToSelectExpression(),
                viewColumn.ToSelectExpression()
            );
        }

        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
        protected virtual void Select<TTable, TView>(Expression<Func<TTable, object>> tableColumn, Expression<Func<TView, object>> viewColumn) => throw new NotImplementedException();

        private static readonly MethodInfo FSelectAvg = GetGenericMethod(self => self.SelectAvg<object, object>(null!, null!));

        public virtual void SelectAvg(PropertyInfo tableColumn, PropertyInfo viewColumn)
        {
            if (tableColumn == null)
                throw new ArgumentNullException(nameof(tableColumn));

            if (viewColumn == null)
                throw new ArgumentNullException(nameof(viewColumn));

            FSelectAvg.MakeGenericMethod(tableColumn.ReflectedType, viewColumn.ReflectedType).Call
            (
                this,
                tableColumn.ToSelectExpression(),
                viewColumn.ToSelectExpression()
            );
        }

        protected virtual void SelectAvg<TTable, TView>(Expression<Func<TTable, object>> tableColumn, Expression<Func<TView, object>> viewColumn) => throw new NotImplementedException();

        private static readonly MethodInfo FSelectCount = GetGenericMethod(self => self.SelectCount<object, object>(null!, null!));

        public virtual void SelectCount(PropertyInfo tableColumn, PropertyInfo viewColumn)
        {
            if (tableColumn == null)
                throw new ArgumentNullException(nameof(tableColumn));

            if (viewColumn == null)
                throw new ArgumentNullException(nameof(viewColumn));

            FSelectCount.MakeGenericMethod(tableColumn.ReflectedType, viewColumn.ReflectedType).Call
            (
                this,
                tableColumn.ToSelectExpression(),
                viewColumn.ToSelectExpression()
            );
        }

        protected virtual void SelectCount<TTable, TView>(Expression<Func<TTable, object>> tableColumn, Expression<Func<TView, object>> viewColumn) => throw new NotImplementedException();

        private static readonly MethodInfo FSelectMax = GetGenericMethod(self => self.SelectMax<object, object>(null!, null!));

        public virtual void SelectMax(PropertyInfo tableColumn, PropertyInfo viewColumn)
        {
            if (tableColumn == null)
                throw new ArgumentNullException(nameof(tableColumn));

            if (viewColumn == null)
                throw new ArgumentNullException(nameof(viewColumn));

            FSelectMax.MakeGenericMethod(tableColumn.ReflectedType, viewColumn.ReflectedType).Call
            (
                this,
                tableColumn.ToSelectExpression(),
                viewColumn.ToSelectExpression()
            );
        }

        protected virtual void SelectMax<TTable, TView>(Expression<Func<TTable, object>> tableColumn, Expression<Func<TView, object>> viewColumn) => throw new NotImplementedException();

        private static readonly MethodInfo FSelectMin= GetGenericMethod(self => self.SelectMin<object, object>(null!, null!));

        public virtual void SelectMin(PropertyInfo tableColumn, PropertyInfo viewColumn)
        {
            if (tableColumn == null)
                throw new ArgumentNullException(nameof(tableColumn));

            if (viewColumn == null)
                throw new ArgumentNullException(nameof(viewColumn));

            FSelectMin.MakeGenericMethod(tableColumn.ReflectedType, viewColumn.ReflectedType).Call
            (
                this,
                tableColumn.ToSelectExpression(),
                viewColumn.ToSelectExpression()
            );
        }

        protected virtual void SelectMin<TTable, TView>(Expression<Func<TTable, object>> tableColumn, Expression<Func<TView, object>> viewColumn) => throw new NotImplementedException();
    }
}
