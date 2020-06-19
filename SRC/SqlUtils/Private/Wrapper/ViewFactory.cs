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

    internal static class ViewFactory
    {
        public static Type CreateView(MemberDefinition viewDefinition, IEnumerable<MemberDefinition> columns)
        {
            TypeBuilder tb = TypeBuilderExtensions.CreateBuilder(viewDefinition.Name);

            //
            // Hogy a GetQueryBase() mukodjon a generalt nezetre is, ezert az uj osztalyt megjeloljuk nezetnek.
            //

            tb.SetCustomAttribute
            (
                new CustomAttributeBuilder
                (
                    typeof(ViewAttribute).GetConstructor(Type.EmptyTypes) ?? throw new MissingMethodException(typeof(ViewAttribute).Name, "Ctor(EmptyTypes)"),
                    constructorArgs: Array.Empty<object?>(),
                    namedProperties: new PropertyInfo[]
                    {
                        typeof(ViewAttribute).GetProperty(nameof(ViewAttribute.Base)) ?? throw new MissingMemberException(typeof(ViewAttribute).Name, nameof(ViewAttribute.Base))
                    },
                    propertyValues: new object[]
                    {
                        viewDefinition.Type
                    }
                )
            );

            foreach (CustomAttributeBuilder cab in viewDefinition.CustomAttributes)
                tb.SetCustomAttribute(cab);

            //
            // Uj property-k definialasa.
            //

            foreach (var column in columns)
            {
                PropertyBuilder property = tb.AddProperty(column.Name, column.Type);

                foreach (CustomAttributeBuilder cab in column.CustomAttributes)
                    property.SetCustomAttribute(cab);
            }

            return tb.CreateTypeInfo()!.AsType();
        }

        public static Type CreateView(MemberDefinition viewDefinition, IEnumerable<ColumnSelection> columns) => CreateView(viewDefinition, columns.Select(col => new MemberDefinition
        (
            col.ViewProperty.Name,
            col.ViewProperty.PropertyType,
            col.Reason.GetBuilder()
        )));
    }
}
