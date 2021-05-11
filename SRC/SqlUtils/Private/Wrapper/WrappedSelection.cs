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

                    type = UnwrappedValueTypeView.CreateView
                    (
                        bta!.OrmType.GetProperty(bta.Column) ?? throw new MissingMemberException(bta.OrmType.Name, bta.Column),
                        bta.Required
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

        public override string ToString() => string.Join(Environment.NewLine, UnderlyingType.ExtractColumnSelections());
    }
}