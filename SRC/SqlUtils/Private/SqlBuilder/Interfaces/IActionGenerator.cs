/********************************************************************************
*  IActionGenerator.cs                                                          *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Solti.Utils.SQL.Internals
{
    internal interface IActionGenerator
    {
        IEnumerable<MethodCallExpression> Generate(ParameterExpression bldr);
    }
}