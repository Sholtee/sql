/********************************************************************************
* IConfig.cs                                                                    *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System.Collections.Generic;
using System.Data;

namespace Solti.Utils.SQL
{
    /// <summary>
    /// Defines the abstract configuration related to this library.
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Stringifies the given parameter.
        /// </summary>
        string Stringify(IDataParameter parameter);

    }
}
