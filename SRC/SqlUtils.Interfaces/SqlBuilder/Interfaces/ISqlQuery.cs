/********************************************************************************
*  ISqlQuery.cs                                                                 *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Represents an abstract SQL builder.
    /// </summary>
    public interface ISqlQuery
    {
        /// <summary>
        /// Extends/creates the select clause with a simple selection.
        /// </summary>
        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords")]
        ISqlQuery Select(PropertyInfo tableColumn, PropertyInfo viewColumn);

        /// <summary>
        /// Extends/creates the select clause with a "COUNT()" selection.
        /// </summary>
        ISqlQuery SelectCount(PropertyInfo tableColumn, PropertyInfo viewColumn);

        /// <summary>
        /// Extends/creates the select clause with a "MAX()" selection.
        /// </summary>
        ISqlQuery SelectMax(PropertyInfo tableColumn, PropertyInfo viewColumn);

        /// <summary>
        /// Extends/creates the select clause with a "MIN()" selection.
        /// </summary>
        ISqlQuery SelectMin(PropertyInfo tableColumn, PropertyInfo viewColumn);

        /// <summary>
        /// Creates a "inner join" clause for this query.
        /// </summary>
        ISqlQuery InnerJoin(PropertyInfo left, PropertyInfo right);

        /// <summary>
        /// Creates a "left join" clause for this query.
        /// </summary>
        ISqlQuery LeftJoin(PropertyInfo left, PropertyInfo right);

        /// <summary>
        /// Extends/creates the "order by" clause of this query.
        /// </summary>
        ISqlQuery OrderBy(PropertyInfo tableColumn);

        /// <summary>
        /// Extends/creates the "order by" clause of this query.
        /// </summary>
        ISqlQuery OrderByDescending(PropertyInfo tableColumn);

        /// <summary>
        /// Extends/creates the "group by" clause of this query.
        /// </summary>
        ISqlQuery GroupBy(PropertyInfo tableColumn);

        /// <summary>
        /// Place for custom finalization routins. This method will be called once the query is assembled.
        /// </summary>
        /// <param name="view"></param>
        void Finalize(Type view);

        /// <summary>
        /// Runs a select query to get a list from the given <paramref name="view"/>.
        /// </summary>
        IList Run(Type view);
    }
}