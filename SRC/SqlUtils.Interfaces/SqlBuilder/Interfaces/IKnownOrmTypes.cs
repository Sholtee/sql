/********************************************************************************
*  IKnownOrmTypes.cs.cs                                                         *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Exposes the ORM types that are intended to be used by this library.
    /// </summary>
    public interface IKnownOrmTypes: IEnumerable<Type>
    {
    }
}