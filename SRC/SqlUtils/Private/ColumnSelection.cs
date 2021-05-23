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
        public ColumnSelection(PropertyInfo viewProperty, SelectionKind kind, ColumnSelectionAttribute reason)
        {
            if (!viewProperty.PropertyType.IsValueTypeOrString())
                throw new NotSupportedException(Resources.CANT_SELECT);

            ViewProperty = viewProperty;
            Kind = kind;
            Reason = reason;
        }

        public PropertyInfo ViewProperty { get; }

        public SelectionKind Kind { get; }

        public ColumnSelectionAttribute Reason { get; }

        public override string ToString() => $"{Reason.OrmType}.{Reason.Column ?? ViewProperty.Name}";
    }
}
