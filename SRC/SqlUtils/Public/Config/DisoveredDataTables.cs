/********************************************************************************
*  DisoveredDataTables.cs                                                       *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Solti.Utils.SQL
{
    using Interfaces;
    using Internals;

    /// <summary>
    /// Enumerates data tables that can be found in assemblies filtered by <see cref="AssemblySearchPattern"/>.
    /// </summary>
    public sealed class DisoveredDataTables : IKnownDataTables
    {
        /// <summary>
        /// The <see cref="Assembly"/> search pattern.
        /// </summary>
        public string AssemblySearchPattern { get; }

        /// <summary>
        /// Creates a new <see cref="DisoveredDataTables"/> instance.
        /// </summary>
        public DisoveredDataTables(string assemblySearchPattern = "*.ORM.dll") => AssemblySearchPattern = assemblySearchPattern ?? throw new ArgumentNullException(nameof(assemblySearchPattern));

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator() => (IEnumerator<Type>) GetEnumerator();

        /// <summary>
        /// Enumerates the known data tables.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            Type[] wouldbeDataTables = 
            (
                from asm in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, AssemblySearchPattern).Select(Assembly.LoadFile)
                from type in asm.GetTypes()
                where Config.Instance.IsDataTable(type)
                select type
            ).ToArray();

            foreach (Type dataTable in wouldbeDataTables)
            {
                dataTable.GetPrimaryKey(); // validal

                //
                // TODO: tobb validalas
                //
            }

            return wouldbeDataTables.GetEnumerator();
        }
    }
}