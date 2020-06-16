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
    using Properties;

    internal sealed class WrappedSelection
    {
        public PropertyInfo Info { get; }

        public Type UnderlyingType { get; }

        public bool IsList { get; }

        public WrappedSelection(PropertyInfo property)
        {
            Debug.Assert(property.IsWrapped());

            Type type = property.PropertyType;

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                //
                // Csak listak tamogatottak
                //

                if (!type.IsList())
                    throw new ArgumentException(Resources.NOT_A_LIST, nameof(property));

                type = type.GetGenericArguments().Single();

                //
                // Listaban ertek tipusok tamogatottak (ezert csak osztaly eseten validalunk):
                //
                // [BelongsTo(typeof(Message), column: "Text")]
                // public List<string> Messages {get; set;}
                //

                if (type.IsClass && !type.IsDatabaseEntityOrView())
                    throw new ArgumentException(Resources.CANT_WRAP, nameof(property));

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
                    throw new ArgumentException(Resources.CANT_WRAP, nameof(property));           
            }

            UnderlyingType = type;
            Info = property;
        }
    }
}