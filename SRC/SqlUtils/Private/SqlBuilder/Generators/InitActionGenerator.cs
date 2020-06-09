/********************************************************************************
*  InitActionGenerator.cs                                                       *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Solti.Utils.SQL.Internals
{
    internal sealed class InitActionGenerator<TView>: ActionGenerator<TView>
    {
        protected override IEnumerable<MethodCallExpression> Generate(ParameterExpression bldr)
        {
            yield return Expression.Call(bldr, QueryMethods.SetBase, Expression.Constant(typeof(TView).GetQueryBase()));
        }
    }
}