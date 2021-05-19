/********************************************************************************
*  ISqlQuery.cs                                                                 *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections;
using System.Data;
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
        void Select(PropertyInfo tableColumn, PropertyInfo viewColumn);

        /// <summary>
        /// Extends/creates the select clause with a "COUNT()" selection.
        /// </summary>
        void SelectCount(PropertyInfo tableColumn, PropertyInfo viewColumn);

        /// <summary>
        /// Extends/creates the select clause with a "MAX()" selection.
        /// </summary>
        void SelectMax(PropertyInfo tableColumn, PropertyInfo viewColumn);

        /// <summary>
        /// Extends/creates the select clause with a "MIN()" selection.
        /// </summary>
        void SelectMin(PropertyInfo tableColumn, PropertyInfo viewColumn);

        /// <summary>
        /// Extends/creates the select clause with an "AVG()" selection.
        /// </summary>
        void SelectAvg(PropertyInfo tableColumn, PropertyInfo viewColumn);

        /// <summary>
        /// Creates an "inner join" clause for this query.
        /// </summary>
        void InnerJoin(PropertyInfo left, PropertyInfo right);

        /// <summary>
        /// Creates a "left join" clause for this query.
        /// </summary>
        void LeftJoin(PropertyInfo left, PropertyInfo right);

        /// <summary>
        /// Extends/creates the "order by" clause of this query.
        /// </summary>
        void OrderBy(PropertyInfo tableColumn);

        /// <summary>
        /// Extends/creates the "order by" clause of this query.
        /// </summary>
        void OrderByDescending(PropertyInfo tableColumn);

        /// <summary>
        /// Extends/creates the "group by" clause of this query.
        /// </summary>
        void GroupBy(PropertyInfo tableColumn);

        /// <summary>
        /// Runs a select query to get a list from the given <paramref name="view"/>.
        /// </summary>
        IList Run(IDbConnection conn, Type view);
    }
}