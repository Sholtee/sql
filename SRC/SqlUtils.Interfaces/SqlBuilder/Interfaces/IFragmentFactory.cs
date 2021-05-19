/********************************************************************************
* IFragmentFactory.cs                                                           *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Creates <see cref="MethodCallExpression"/>s to represent <see cref="ISqlQuery"/> invocations.
    /// </summary>
    public interface IFragmentFactory
    {
        /// <summary>
        /// Gets the query fragments.
        /// </summary>
        IEnumerable<MethodCallExpression> GetFragments(ParameterExpression bldr, PropertyInfo viewProperty, bool isGroupBy);
    }
}
