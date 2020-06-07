/********************************************************************************
*  Compiler.cs                                                                  *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal static class Compiler
    {
        public static Action<ISqlQuery> Compile(params IActionGenerator[] generators)
        {
            ParameterExpression bldr = Expression.Parameter(typeof(ISqlQuery), nameof(bldr));

            return Expression
                .Lambda<Action<ISqlQuery>>(Expression.Block(generators.SelectMany(fragments => fragments.Generate(bldr))), bldr)
                .Compile();
        }
    }
}
