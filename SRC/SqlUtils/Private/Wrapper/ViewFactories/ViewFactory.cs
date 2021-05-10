/********************************************************************************
*  ViewFactory.cs                                                               *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal class ViewFactory: ClassFactory
    {
        public ViewFactory(MemberDefinition viewDefinition, IEnumerable<MemberDefinition> columns) : base(viewDefinition.Name)
        {
            //
            // Hogy a GetQueryBase() mukodjon a generalt nezetre is, ezert az uj osztalyt megjeloljuk nezetnek.
            //

            Class.SetCustomAttribute
            (
                CustomAttributeBuilderFactory.CreateFrom<ViewAttribute>(new[] { typeof(Type) }, new object?[] { viewDefinition.Type })
            );

            foreach (CustomAttributeBuilder cab in viewDefinition.CustomAttributes)
            {
                Class.SetCustomAttribute(cab);
            }

            //
            // Uj property-k definialasa.
            //

            foreach (MemberDefinition column in columns)
            {
                PropertyBuilder property = AddProperty(column.Name, column.Type);

                foreach (CustomAttributeBuilder cab in column.CustomAttributes)
                {
                    property.SetCustomAttribute(cab);
                }
            }
        }
    }
}
