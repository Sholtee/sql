/********************************************************************************
*  ViewFactory.cs                                                               *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal class ViewFactory
    {
        internal protected static Type CreateView(MemberDefinition viewDefinition, IEnumerable<MemberDefinition> columns)
        {
            ClassFactory core = new
            (
                viewDefinition.Name,
                viewDefinition
                    .CustomAttributes

                    //
                    // Hogy a GetQueryBase() mukodjon a generalt nezetre is, ezert az uj osztalyt megjeloljuk nezetnek.
                    //

                    .Append(CustomAttributeBuilderFactory.CreateFrom<ViewAttribute>(new[] { typeof(Type) }, new object?[] { viewDefinition.Type }))
                    .ToArray()
            );

            //
            // Uj property-k definialasa.
            //

            foreach (MemberDefinition column in columns)
            {
                core.AddProperty(column.Name, column.Type, column.CustomAttributes.ToArray());
            }

            return core.CreateType();
        }
    }
}
