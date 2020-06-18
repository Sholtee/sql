/********************************************************************************
*  ViewFactory.cs                                                               *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal static class ViewFactory
    {
        public static Type CreateView(string name, Type @base, IEnumerable<ColumnSelection> columns)
        {
            TypeBuilder tb = MyTypeBuilder.Create(name);

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
                        @base
                    }
                )
            );

            //
            // Property-k masolasa.
            //

            foreach (ColumnSelection sel in columns)
            {
                tb
                    .AddProperty(sel.Column)
                    .SetCustomAttribute(sel.Reason.GetBuilder());
            }

            return tb.CreateTypeInfo()!.AsType();
        }
    }
}
