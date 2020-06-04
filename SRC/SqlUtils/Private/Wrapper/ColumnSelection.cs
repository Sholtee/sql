/********************************************************************************
*  ColumnSelection.cs                                                           *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/

using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal sealed class ColumnSelection
    {
        public ColumnSelection(PropertyInfo column, SelectionKind kind, ColumnSelectionAttribute reason)
        {
            Column = column;
            Kind   = kind;
            Reason = reason;
        }

        public PropertyInfo Column { get; }
        public SelectionKind Kind { get; }
        public ColumnSelectionAttribute Reason { get; }
    }
}
