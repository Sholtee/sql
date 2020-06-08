/********************************************************************************
* FakeTables.cs                                                                 *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;

namespace Solti.Utils.SQL.Tests
{
    using Interfaces;
    
    public sealed class FakeTables : IKnownDataTables
    {
        private readonly Type[] FTypes;

        public FakeTables(params Type[] types) => FTypes = types;

        public IEnumerator<Type> GetEnumerator() => ((IEnumerable<Type>) FTypes).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}