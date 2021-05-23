/********************************************************************************
* PropertyInfoExtensions.cs                                                     *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Diagnostics;
using System.Linq;
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

        public static object? FastGetValue(this PropertyInfo src, object instance) => src
            .ToGetter()
            .Invoke(instance);

        public static void FastSetValue(this PropertyInfo src, object instance, object? value) => src
            .ToSetter()
            .Invoke(instance, value);

        public static string FullName(this PropertyInfo src) => $"{src.ReflectedType.FullName}.{src.Name}";

        public static bool IsRedirectedTo(this PropertyInfo src, PropertyInfo dst) => src.GetCustomAttribute<MapToAttribute>()?.Property == dst.FullName();

        public static bool CanBeMappedIn(this PropertyInfo src, PropertyInfo dst) => (src.IsRedirectedTo(dst) || src.Name == dst.Name) && dst.PropertyType.IsAssignableFrom(src.PropertyType);

        public static ColumnSelection? AsColumnSelection(this PropertyInfo prop, bool baseRequired)
        {
            Debug.Assert(!prop.IsWrapped());

            ColumnSelectionAttribute csa = prop.GetCustomAttribute<ColumnSelectionAttribute>();
            if (csa is not null)
                return new ColumnSelection
                (
                    viewProperty: prop,
                    kind: SelectionKind.Explicit,
                    reason: csa
                );

            //
            // - Ha a nezet egy mar meglevo adattabla leszarmazottja akkor azon property-ket
            //   is kivalasztjuk melyek az os entitashoz tartoznak.
            //
            // - Az h ignoralva van e a property csak adatbazis entitasnal kell vizsgaljuk (nezetnel
            //   ha nincs ColumnSelectionAttribute rajt akkor automatikusan ignoralt).
            // 

            Type? databaseEntity = prop.ReflectedType.GetBaseDataTable();
            if (databaseEntity is not null && !Config.Instance.IsIgnored(prop) && prop.DeclaringType.IsAssignableFrom(databaseEntity))
                return new ColumnSelection
                (
                    viewProperty: prop,
                    kind: SelectionKind.Implicit,
                    reason: new BelongsToAttribute(databaseEntity, baseRequired)
                );

            //
            // Hat ez nem sikerult...
            //

            return null;
        }

        public static Type GetEffectiveType(this PropertyInfo prop) => Cache.GetOrAdd(prop, () =>
        {
            Type type = prop.PropertyType;

            if (type.IsList())
            {
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
                    BelongsToAttribute bta = prop.GetCustomAttribute<BelongsToAttribute>();

                    type = UnwrappedValueTypeView.CreateView(bta);
                }
            }

            return type;
        });
    }
}
