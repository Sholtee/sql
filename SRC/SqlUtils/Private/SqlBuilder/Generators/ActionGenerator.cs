/********************************************************************************
*  ActionGenerator.cs                                                           *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Solti.Utils.SQL.Internals
{
    internal abstract class ActionGenerator<TView> : IActionGenerator
    {
        //
        // Ha van lista tulajdonsag a nezetben akkor a nezetet le kell cserelni listakat nem
        // tartalmazo parjara.
        //

        protected static readonly IReadOnlyList<ColumnSelection> Selections = Unwrapped<TView>
            .Type
            .GetColumnSelections();

        IEnumerable<MethodCallExpression> IActionGenerator.Generate(ParameterExpression bldr) => Generate(bldr);

        protected abstract IEnumerable<MethodCallExpression> Generate(ParameterExpression bldr);
    }
}