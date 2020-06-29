/********************************************************************************
*  ViewFactory.cs                                                               *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal class ViewFactory: ClassFactory
    {
        protected static Type Create(MemberDefinition viewDefinition, IEnumerable<MemberDefinition> columns)
        {
            TypeBuilder tb = CreateBuilder(viewDefinition.Name);

            //
            // Hogy a GetQueryBase() mukodjon a generalt nezetre is, ezert az uj osztalyt megjeloljuk nezetnek.
            //

            tb.SetCustomAttribute
            (
                CustomAttributeBuilderFactory.CreateFrom<ViewAttribute>(new[] { typeof(Type) }, new object?[] { viewDefinition.Type })
            );

            foreach (CustomAttributeBuilder cab in viewDefinition.CustomAttributes)
            {
                tb.SetCustomAttribute(cab);
            }

            //
            // Uj property-k definialasa.
            //

            foreach (MemberDefinition column in columns)
            {
                PropertyBuilder property = AddProperty(tb, column.Name, column.Type);

                foreach (CustomAttributeBuilder cab in column.CustomAttributes)
                {
                    property.SetCustomAttribute(cab);
                }
            }

            return tb.CreateTypeInfo()!.AsType();
        }

        protected static CustomAttributeBuilder[] CopyAttributes(MemberInfo member) => member
            .GetCustomAttributes()
            .OfType<IBuildableAttribute>()
            .Select(attr => CustomAttributeBuilderFactory.CreateFrom(attr))
            .ToArray();
    }
}
