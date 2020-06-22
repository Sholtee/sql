/********************************************************************************
* IBuildableAttribute.cs                                                        *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Interfaces
{
    /// <summary>
    /// Represents an attribute that can be built.
    /// </summary>
    public interface IBuildableAttribute
    {
        /// <summary>
        /// Gets the <see cref="CustomAttributeBuilder"/> that builds the <see cref="Attribute"/> that implements this interface.
        /// </summary>
        CustomAttributeBuilder GetBuilder(params KeyValuePair<PropertyInfo, object>[] customParameters);
    }
}
