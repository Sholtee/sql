/********************************************************************************
* IFragment.cs                                                                  *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL
{
    /// <summary>
    /// Represents one or more query fragments.
    /// </summary>
    public interface IFragment
    {
        /// <summary>
        /// Gets the query fragments.
        /// </summary>
        IEnumerable<MethodCallExpression> GetFragments(ParameterExpression bldr, PropertyInfo viewProperty, bool isGroupBy);
    }
}
