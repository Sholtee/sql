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

    internal class ViewFactory: ClassFactory
    {
        public static Type CreateView(MemberDefinition viewDefinition, IEnumerable<MemberDefinition> columns) => Cache.GetOrAdd(viewDefinition.Name, () =>
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
        });

        public static Type CreateViewForValueType(PropertyInfo dataTableColumn, bool required) => Cache.GetOrAdd(dataTableColumn, () =>
        {
            Type dataTable = dataTableColumn.ReflectedType;
            PropertyInfo pk = dataTable.GetPrimaryKey();

            //
            // [View(Base = typeof(Table)), MapFrom(nameof(Column))]
            // class Table_Column_View
            // {
            //   [BelongsTo(typeof(Table))]
            //   public int Id {get; set;}
            //   [BelongsTo(typeof(Table))]
            //   public ValueType Column {get; set;}
            // }
            //

            return CreateView
            (
                new MemberDefinition
                (
                    $"{dataTable.Name}_{dataTableColumn.Name}_View",
                    dataTable,
                    CustomAttributeBuilderFactory.CreateFrom<MapFromAttribute>(new[] { typeof(string) }, new object[] { dataTableColumn.Name })
                ),
                new[]
                {
                    //
                    // [BelongsTo(typeof(Table))]
                    // public int Id {get; set;}
                    //

                    new MemberDefinition
                    (
                        pk.Name,
                        pk.PropertyType,
                        CustomAttributeBuilderFactory.CreateFrom(new BelongsToAttribute(dataTable, required))
                    ),

                    //
                    // [BelongsTo(typeof(Table), column: "Column")]
                    // public ValueType Column {get; set;}
                    //

                    new MemberDefinition
                    (
                        dataTableColumn.Name,
                        dataTableColumn.PropertyType,
                        CustomAttributeBuilderFactory.CreateFrom(new BelongsToAttribute(dataTable, required))
                    )
                }
            );
        });
    }
}
