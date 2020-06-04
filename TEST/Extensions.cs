/********************************************************************************
*  Extensions.cs                                                                *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    using Internals;

    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, Action<T> action)
        {
            return src.ForEach((x, i) => action(x));
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> src, Action<T, int> action)
        {
            src = src as T[] ?? src.ToArray(); // Ne dumaljon a fordito a tobbszoros felsorolas miatt

            int i = 0;
            IEnumerator<T> enumerator = src.GetEnumerator();
            while (enumerator.MoveNext()) action(enumerator.Current, i++);

            Assert.That(i, Is.GreaterThan(0));

            return src;
        }
    }

    internal static class ObjectExtensions
    {
        public static object Set(this object src, string prop, object value)
        {
            src.GetType().GetProperty(prop).FastSetValue(src, value);
            return src;
        }
    }
}