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
        #pragma warning disable CA1002 // Do not expose generic lists
        public static List<TView> Run<TView>(this ISqlQuery query)
        #pragma warning restore CA1002
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (typeof(TView).IsWrapped())
            {
                IList result = query.Run(UnwrappedView<TView>.Type);

                //
                // Ha az eredmeny NULL akkor nincs dolgunk (+ a Wrap() is elhasalna tole)
                //

                return result == null ? new List<TView>(0) : Wrapper<TView>.Wrap(result);
            }

            return (List<TView>) query.Run(typeof(TView));
        }
    }
}