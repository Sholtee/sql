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
    using Primitives;

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

        public static Type CreateViewForValueType(PropertyInfo dataTableColumn) => Cache.GetOrAdd(dataTableColumn, () =>
        {
            Type dataTable = dataTableColumn.ReflectedType;
            PropertyInfo pk = dataTable.GetPrimaryKey();

            //
            // [View(Base = typeof(Table)), MapFrom(nameof(Column))]
            // class Table_Column_View
            // {
            //   [BelongsTo(typeof(Table), column: "Id")]
            //   public int Table_Id {get; set;}
            //   [BelongsTo(typeof(Table), column: "Column")]
            //   public ValueType Column {get; set;}
            // }
            //

            return CreateView
            (
                new MemberDefinition
                (
                    $"{dataTable.Name}_{dataTableColumn.Name}_View",
                    dataTable,
                    new CustomAttributeBuilder
                    (
                        typeof(MapFromAttribute).GetConstructor(new[] { typeof(string) }) ?? throw new MissingMethodException(typeof(MapFromAttribute).Name, "Ctor(string)"),
                        constructorArgs: new object[] { dataTableColumn.Name }
                    )
                ),
                new[]
                {
                    //
                    // [BelongsTo(typeof(Table), column: "Id")]
                    // public int Table_Id {get; set;}
                    //

                    new MemberDefinition
                    (
                        $"{dataTable.Name}_{pk.Name}", // direkt nem csak pk.Name h kissebb esellyel legyen nev utkozes
                        pk.PropertyType,
                        new BelongsToAttribute(dataTable, required: false, column: pk.Name).GetBuilder()
                    ),

                    //
                    // [BelongsTo(typeof(Table), column: "Column")]
                    // public ValueType Column {get; set;}
                    //

                    new MemberDefinition
                    (
                        dataTableColumn.Name,
                        dataTableColumn.PropertyType,
                        new BelongsToAttribute(dataTable, required: false).GetBuilder()
                    )
                }
            );
        });
    }
}
