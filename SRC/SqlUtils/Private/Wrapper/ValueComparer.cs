/********************************************************************************
* ValueComparer.cs                                                              *
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
    internal sealed class ValueComparer : IEqualityComparer<object>
    {
        public static readonly IEqualityComparer<object> Instance = new ValueComparer();

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            if (ReferenceEquals(x, y)) return true;

            if (x == null || y == null) return false;

            if (x.GetType() != y.GetType()) return false;

            //
            // Ha az Equals() felul lett irva (pl. primitiv, anonim tipusok) akkor azt hasznaljuk.
            //

            if (x.GetType().HasOwnMethod(nameof(Equals), typeof(object)))
                return x.Equals(y);

            return GetHashCode(x) == GetHashCode(y);
        }

        public int GetHashCode(object obj)
        {
            if (obj == null) return 0;

            Type type = obj.GetType();

            //
            // Ha van sajat GetHashCode()-ja akkor azt hasznaljuk.
            //

            if (type.HasOwnMethod(nameof(GetHashCode)))
                return obj.GetHashCode();

            var hc = new HashCode();

            if (obj is IEnumerable @enum)
                foreach (object i in @enum) 
                    hc.Add(i, this);
            else 
                foreach (PropertyInfo prop in type.GetProperties().Where(p => p.CanRead))
                    hc.Add(prop.FastGetValue(obj), this);

            return hc.ToHashCode();
        }
    }
}