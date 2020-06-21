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

    /// <summary>
    /// Enumerates data tables that can be found in assemblies.
    /// </summary>
    public sealed class DisoveredDataTables : IKnownDataTables
    {
        private readonly IReadOnlyList<Assembly> FAssemblies;

        /// <summary>
        /// Creates a new <see cref="DisoveredDataTables"/> instance.
        /// </summary>
        public DisoveredDataTables(string assemblySearchPattern = "*.ORM.dll") => FAssemblies = Directory
            .GetFiles
            (
                AppDomain.CurrentDomain.BaseDirectory, assemblySearchPattern ?? throw new ArgumentNullException(nameof(assemblySearchPattern))
            )
            .Select(Assembly.LoadFile)
            .ToArray();

        /// <summary>
        /// Creates a new <see cref="DisoveredDataTables"/> instance.
        /// </summary>
        public DisoveredDataTables(params Assembly[] assemblies) => FAssemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator() => (IEnumerator<Type>) GetEnumerator();

        /// <summary>
        /// Enumerates the known data tables.
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            IEnumerable<Type> wouldbeDataTables = 
            (
                from asm in FAssemblies
                from type in asm.GetTypes()
                where Config.Instance.IsDataTable(type)
                select type
            );

            //
            // TODO: validalas
            //

            return wouldbeDataTables.GetEnumerator();
        }
    }
}