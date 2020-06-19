/********************************************************************************
* WrappedSelection.cs                                                           *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;
    using Properties;

    internal sealed class WrappedSelection: ISelection
    {
        public PropertyInfo ViewProperty { get; }

        public Type UnderlyingType { get; }

        public bool IsList { get; }

        public WrappedSelection(PropertyInfo viewProperty)
        {
            Debug.Assert(viewProperty.IsWrapped());

            Type type = viewProperty.PropertyType;

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                //
                // Csak listak tamogatottak
                //

                if (!type.IsList())
                    throw new ArgumentException(Resources.NOT_A_LIST, nameof(viewProperty));

                type = type.GetGenericArguments().Single();

                //
                // [BelongsTo(typeof(Message), column: "Text")]
                // public List<string> Messages {get; set;}
                //
                // List<ValueType> eseten letrehozunk egy belso nezetet amiben szerepel a join-olt tabla
                // elsodleges kulcsa is.
                //
                
                if (type.IsValueTypeOrString())
                {
                    BelongsToAttribute bta = viewProperty.GetCustomAttribute<BelongsToAttribute>();
                    Debug.Assert(bta != null, "[List<ValueType> Prop] must have BelongsToAttribute");

                    type = CreateViewForValueType
                    (
                        bta!.OrmType.GetProperty(bta.Column) ?? throw new MissingMemberException(bta.OrmType.Name, bta.Column)
                    );
                }

                IsList = true;
            }

            //
            // [Wrapped]
            // List<Message> Messages {get; set;}
            //
            // vagy
            //
            // [Wrapped]
            // User User {get; set;}
            //

            if (!type.IsDatabaseEntityOrView())
                throw new ArgumentException(Resources.CANT_WRAP, nameof(viewProperty));

            UnderlyingType = type;
            ViewProperty = viewProperty;
        }

        private static Type CreateViewForValueType(PropertyInfo dataTableColumn) => Cache.GetOrAdd(dataTableColumn, () =>
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

            return ViewFactory.CreateView
            (
                new MemberDefinition
                (
                    $"{dataTable.Name}_{dataTableColumn.Name}_View",
                    dataTable,
                    new CustomAttributeBuilder
                    (
                        typeof(MapFromAttribute).GetConstructor(new[]{ typeof(string) }) ?? throw new MissingMethodException(typeof(MapFromAttribute).Name, "Ctor()"),
                        constructorArgs: new object[]{ dataTableColumn.Name }
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
                        $"{dataTable.Name}_{pk.Name}", 
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

        public override string ToString() => string.Join(Environment.NewLine, UnderlyingType.ExtractColumnSelections());
    }
}