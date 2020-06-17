/********************************************************************************
* PropertyInfoExtensions.cs                                                     *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;

    internal static class PropertyInfoExtensions
    {
        //
        // Nem validal
        //

        public static bool IsWrapped(this PropertyInfo prop) => prop.GetCustomAttribute<WrappedAttribute>() != null || (prop.GetCustomAttribute<BelongsToAttribute>() != null && prop.PropertyType.IsList());

        public static object FastGetValue(this PropertyInfo src, object instance) => src
            .ToGetter()
            .Invoke(instance);

        public static void FastSetValue(this PropertyInfo src, object instance, object? value) => src
            .ToSetter()
            .Invoke(instance, value);
    }
}
