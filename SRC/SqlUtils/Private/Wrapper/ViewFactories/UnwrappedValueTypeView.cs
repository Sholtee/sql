/********************************************************************************
*  UnwrappedValueTypeView.cs                                                    *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;

    internal class UnwrappedValueTypeView: ViewFactory
    {
        public static Type CreateView(PropertyInfo dataTableColumn, bool required) => Cache.GetOrAdd(dataTableColumn, () =>
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
        }, nameof(UnwrappedValueTypeView));
    }
}
