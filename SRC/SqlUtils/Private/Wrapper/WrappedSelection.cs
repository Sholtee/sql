/********************************************************************************
* WrappedSelection.cs                                                           *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
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
            Type type = property.PropertyType;

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(List<>))
                    throw new ArgumentException(Resources.NOT_A_LIST, nameof(property));

                UnderlyingType = type.GetGenericArguments().Single();
                IsList = true;
            }
            else 
                UnderlyingType = type;

            Info = property;
        }
    }
}