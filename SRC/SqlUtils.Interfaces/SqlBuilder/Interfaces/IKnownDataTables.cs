/********************************************************************************
*  IKnownDataTables.cs                                                          *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Exposes data tables (represented by <see cref="Type"/>) that are intended to be used by this library.
    /// </summary>
    public interface IKnownDataTables: IEnumerable<Type>
    {
    }
}