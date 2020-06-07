/********************************************************************************
* ISqlQueryExtensions.cs                                                        *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;

namespace Solti.Utils.SQL
{
    using Interfaces;
    using Internals;

    /// <summary>
    ///
    /// </summary>
    public static class ISqlQueryExtensions
    {
        /// <summary>
        /// Queries the given view.
        /// </summary>
        public static List<TView> Select<TView>(this ISqlQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (typeof(TView).IsWrapped())
            {
                IList result = query.Run(Unwrapped<TView>.Type);

                //
                // Ha az eredmeny NULL akkor nincs dolgunk (+ a Wrap() is elhasalna tole)
                //

                return result == null ? new List<TView>(0) : Wrapper.Wrap<TView>(result);
            }

            return (List<TView>) query.Run(typeof(TView));
        }
    }
}