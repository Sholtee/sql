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
        public static Type CreateView(BelongsToAttribute bta) => Cache.GetOrAdd(bta /*jo kulcsnak*/, () =>
        {
            Type dataTable = bta.OrmType;

            PropertyInfo 
                pk = dataTable.GetPrimaryKey(),
                column = bta.OrmType.GetProperty(bta.Column) ?? throw new MissingMemberException(bta.OrmType.Name, bta.Column);

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
                    //
                    // A hash kod kell a tipus nevebe mivel ugyanazon oszlophoz tartozo erteklista szerepelhet tobb nezetben is
                    // kulonbozo "required" ertekkel.
                    //

                    $"{dataTable.Name}_{column.Name}_View_{bta.GetHashCode()}", // TODO: FIXME: bta.GetHashCode() gyanusan sokszor ad vissza 0-t
                    dataTable,
                    CustomAttributeBuilderFactory.CreateFrom<MapFromAttribute>(new[] { typeof(string) }, new object[] { column.Name })
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
                        CustomAttributeBuilderFactory.CreateFrom(new BelongsToAttribute(dataTable, bta.Required))
                    ),

                    //
                    // [BelongsTo(typeof(Table), column: "Column")]
                    // public ValueType Column {get; set;}
                    //

                    new MemberDefinition
                    (
                        column.Name,
                        column.PropertyType,
                        CustomAttributeBuilderFactory.CreateFrom(new BelongsToAttribute(dataTable, bta.Required))
                    )
                }
            );
        }, nameof(UnwrappedValueTypeView));
    }
}
