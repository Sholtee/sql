/********************************************************************************
*  ColumnSelection.cs                                                           *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Properties;

    internal sealed class ColumnSelection
    {
        public ColumnSelection(PropertyInfo column, SelectionKind kind, ColumnSelectionAttribute reason)
        {
            if (!column.PropertyType.IsValueType && column.PropertyType != typeof(string))
                throw new NotSupportedException(Resources.CANT_SELECT);

            Column = column;
            Kind   = kind;
            Reason = reason;
        }

        public PropertyInfo Column { get; }
        public SelectionKind Kind { get; }
        public ColumnSelectionAttribute Reason { get; }

        public override string ToString() => $"{Reason.OrmType}.{Reason.OrmType.GetProperty(Reason.Column ?? Column.Name).Name}";
    }
}
