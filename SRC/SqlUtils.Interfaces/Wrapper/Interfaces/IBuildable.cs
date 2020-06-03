/********************************************************************************
* IBuildable.cs                                                                 *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Represents an attribute that can be built.
    /// </summary>
    public interface IBuildable
    {
        /// <summary>
        /// The related <see cref="CustomAttributeBuilder"/>.
        /// </summary>
        CustomAttributeBuilder Builder { get; }
    }
}
