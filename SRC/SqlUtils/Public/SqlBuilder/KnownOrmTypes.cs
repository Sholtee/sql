/********************************************************************************
*  KnownOrmTypes.cs                                                             *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Solti.Utils.SQL.Interfaces
{
    using Primitives;

    /// <summary>
    /// The default implementation of the <see cref="IKnownOrmTypes"/> interface.
    /// </summary>
    public sealed class KnownOrmTypes : IKnownOrmTypes
    {
        /// <summary>
        /// The <see cref="Assembly"/> search pattern.
        /// </summary>
        public static string AssemblySearchPattern { get; set; } = "*.ORM.dll";

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator() => (IEnumerator<Type>) GetEnumerator();

        /// <summary>
        /// Enumerates the known ORM types.
        /// </summary>
        public IEnumerator GetEnumerator() => Cache.GetOrAdd(nameof(KnownOrmTypes), () =>
        (
            from asm in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, AssemblySearchPattern).Select(Assembly.LoadFile)
            from type in asm.GetTypes()
            where Config.Instance.IsDatabaseEntity(type)
            select type
        ).ToArray()).GetEnumerator();
    }
}