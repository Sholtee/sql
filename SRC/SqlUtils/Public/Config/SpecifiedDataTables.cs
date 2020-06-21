/********************************************************************************
* SpecifiedDataTables.cs                                                        *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;

namespace Solti.Utils.SQL
{
    using Interfaces;
    using Internals;
    
    /// <summary>
    /// Lets you register data tables manually.
    /// </summary>
    public sealed class SpecifiedDataTables : IKnownDataTables
    {
        private readonly IReadOnlyList<Type> FDataTables;

        /// <summary>
        /// Creates a new <see cref="SpecifiedDataTables"/> instance.
        /// </summary>
        public SpecifiedDataTables(params Type[] dataTables)
        {
            if (dataTables == null)
                throw new ArgumentNullException(nameof(dataTables));

            foreach (Type dataTable in dataTables)
            {
                dataTable.GetPrimaryKey(); // validal

                //
                // TODO: tobb validalas
                //
            }

            FDataTables = dataTables;
        }

        /// <summary>
        /// Enumerates the specified data tables.
        /// </summary>
        public IEnumerator<Type> GetEnumerator() => FDataTables.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}