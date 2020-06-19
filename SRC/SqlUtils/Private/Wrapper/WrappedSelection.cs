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
                // Listaban ertek tipusok tamogatottak (ezert csak osztaly eseten validalunk):
                //
                // [BelongsTo(typeof(Message), column: "Text")]
                // public List<string> Messages {get; set;}
                //

                if (!type.IsValueTypeOrString() && !type.IsDatabaseEntityOrView())
                    throw new ArgumentException(Resources.CANT_WRAP, nameof(viewProperty));

                IsList = true;
            }
            else
            {
                //
                // Ha nem listaban van akkor mindenkepp csomagolhatonak kell lennie:
                //
                // [Wrapped]
                // public string Messages {get; set;}  // !!INVALID!!
                //

                if (!type.IsDatabaseEntityOrView())
                    throw new ArgumentException(Resources.CANT_WRAP, nameof(viewProperty));           
            }

            UnderlyingType = type;
            ViewProperty = viewProperty;
        }

        private static Type CreateViewForValueType(Type valueType, PropertyInfo dataTableColumn) 
        {
            Debug.Assert(valueType.IsValueTypeOrString());

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
                    new[]
                    {
                        new CustomAttributeBuilder
                        (
                            typeof(MapFromAttribute).GetConstructor(new[]{ typeof(string) }) ?? throw new MissingMethodException(typeof(MapFromAttribute).Name, "Ctor()"),
                            constructorArgs: new object[]{ dataTableColumn.Name }
                        )
                    }
                ),
                new[]
                {
                    //
                    //   [BelongsTo(typeof(Table), column: "Id")]
                    //   public int Table_Id {get; set;}
                    //

                    new MemberDefinition($"{dataTable.Name}_{pk.Name}", pk.PropertyType, new BelongsToAttribute(dataTable, column: pk.Name).GetBuilder()),

                    //
                    // [BelongsTo(typeof(Table), column: "Column")]
                    // public ValueType Column {get; set;}
                    //

                    new MemberDefinition(dataTableColumn.Name, dataTableColumn.PropertyType, new BelongsToAttribute(dataTable).GetBuilder())
                }
            );
        }

        public override string ToString() => string.Join(Environment.NewLine, UnderlyingType.ExtractColumnSelections());
    }
}