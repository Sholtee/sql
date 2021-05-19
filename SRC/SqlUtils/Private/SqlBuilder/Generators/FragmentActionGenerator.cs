/********************************************************************************
*  FragmentActionGenerator.cs                                                   *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal sealed class FragmentActionGenerator<TView>: ActionGenerator<TView>
    {
        private static readonly bool IsAggregate = Selections
            .Any(col => col.Reason is AggregateSelectionAttribute);

        protected override IEnumerable<MethodCallExpression> Generate(ParameterExpression bldr)
        {
            foreach(ColumnSelection sel in Selections)
            {
                IFragmentFactory fragment = sel.Reason;

                foreach (MethodCallExpression action in fragment.GetFragments(bldr, sel.ViewProperty, IsAggregate))
                {
                    yield return action;
                }
            }
        }
    }
}