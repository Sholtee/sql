/********************************************************************************
*  FinalActionGenerator.cs                                                      *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Solti.Utils.SQL.Internals
{
    internal sealed class FinalActionGenerator<TView>: IActionGenerator
    {
        IEnumerable<MethodCallExpression> IActionGenerator.Generate(ParameterExpression bldr)
        {
            yield return Expression.Call(bldr, QueryMethods.Finalize, Expression.Constant(typeof(TView)));
        }
    }
}