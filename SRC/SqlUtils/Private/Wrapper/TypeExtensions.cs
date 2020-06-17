/********************************************************************************
* TypeExtensions.cs                                                             *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;
    using Properties;

    internal static partial class TypeExtensions
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public static IReadOnlyList<WrappedSelection> GetWrappedSelections(this Type viewOrDatabaseEntity) => Cache.GetOrAdd
        (
            viewOrDatabaseEntity, 
            () => viewOrDatabaseEntity.IsValueTypeOrString()
                ? Array.Empty<WrappedSelection>()
                : viewOrDatabaseEntity
                    .GetProperties(BINDING_FLAGS)
                    .Where(PropertyInfoExtensions.IsWrapped)
                    .Select(prop => new WrappedSelection(prop))
                    .ToArray()
        );

        public static bool IsWrapped(this Type src) => src.GetWrappedSelections().Any();

        public static IReadOnlyList<ColumnSelection> GetColumnSelections(this Type viewOrDatabaseEntity) => Cache.GetOrAdd(viewOrDatabaseEntity, () => 
        {
            if (viewOrDatabaseEntity.IsValueTypeOrString()) return Array.Empty<ColumnSelection>();

            Type? @base = viewOrDatabaseEntity.GetBaseDataType();

            return 
            (
                from prop in viewOrDatabaseEntity.GetProperties(BINDING_FLAGS)
                where !prop.IsWrapped()

                //
                // - Ha a nezet egy mar meglevo adattabla leszarmazottja akkor azon property-ket
                //   is kivalasztjuk melyek az os entitashoz tartoznak.
                //
                // - Az h ignoralva van e a property csak adatbazis entitasnal kell vizsgaljuk (nezetnel
                //   ha nincs ColumnSelectionAttribute rajt akkor automatikusan ignoralt).
                // 

                let attr = prop.GetCustomAttribute<ColumnSelectionAttribute>()
                where attr != null || (@base != null && !Config.Instance.IsIgnored(prop) && prop.DeclaringType.IsAssignableFrom(@base))

                select new ColumnSelection
                (
                    column: prop,
                    kind: attr != null ? SelectionKind.Explicit : SelectionKind.Implicit,
                    reason: attr ?? new BelongsToAttribute(@base!)
                )
            ).ToArray();
        });

        public static IReadOnlyList<ColumnSelection> ExtractColumnSelections(this Type viewOrDatabaseEntity) => Cache.GetOrAdd(viewOrDatabaseEntity, () =>
        {
            if (viewOrDatabaseEntity.IsValueTypeOrString()) return Array.Empty<ColumnSelection>();

            ColumnSelection[] result = viewOrDatabaseEntity
                .GetColumnSelections()
                .Concat(viewOrDatabaseEntity
                    .GetWrappedSelections()
                    .SelectMany(sel =>
                    {
                        Debug.Assert(sel.UnderlyingType.IsDatabaseEntityOrView());

                        //
                        // [Wrapped]
                        // public List<Message> Messages {get; set;}
                        //
                        // ->
                        //
                        // [BelongsTo(typeof(Message))]
                        // public string Text {get; set;}
                        //
                        // [BelongsTo(typeof(Message))]
                        // public xXx OtherPropr {get; set;}
                        //

                        return sel.UnderlyingType.ExtractColumnSelections();
                    }))
                .ToArray();

            //
            // class A {int Foo;}
            // class B {int Foo;}
            // class C
            // {
            //    A A;
            //    B B;
            // }
            //

            string[] collisions =
            (
                from   sel in result
                group  sel by sel.Column.Name into grp
                where  grp.Count() > 1
                select grp.Key
            ).ToArray();
                
            if (collisions.Any())
            {
                var ex = new InvalidOperationException(Resources.PROPERTY_NAME_COLLISSION);
                ex.Data[nameof(collisions)] = collisions;
                throw ex;
            }

            return result;
        });

        public static Type? GetBaseDataType(this Type viewOrDatabaseEntity)
        {
            while (viewOrDatabaseEntity != null && !Config.Instance.IsDataTable(viewOrDatabaseEntity))
            {
                viewOrDatabaseEntity = viewOrDatabaseEntity.BaseType;
            }
            return viewOrDatabaseEntity;
        }

        public static bool IsValueTypeOrString(this Type src) => src.IsPrimitive || src == typeof(string);

        public static bool IsDatabaseEntityOrView(this Type type) => type.IsClass && (type.GetCustomAttribute<ViewAttribute>(inherit: false) ?? (object?) type.GetBaseDataType()) != null;

        public static object GetDefaultValue(this Type src) => Cache
            .GetOrAdd(src, () => Expression
                .Lambda<Func<object>>(Expression.Convert(Expression.Default(src), typeof(object)))
                .Compile())
            .Invoke();

        public static PropertyInfo? GetEmptyListMarker(this Type view) => Cache.GetOrAdd(view, () =>
        {
            if (view.IsValueTypeOrString()) return null;

            try
            {
                return view
                    .GetColumnSelections()
                    .SingleOrDefault(sel => sel.Column.GetCustomAttribute<EmptyListMarkerAttribute>() != null)
                    ?.Column;
            }
            catch (InvalidOperationException) { throw new InvalidOperationException(Resources.MULTIPLE_EMPTY_LIST_MARKER); }
        });

        public static object MakeInstance(this Type src, params Type[] typeArguments)
        {
            if (typeArguments.Any())
                src = src.MakeGenericType(typeArguments);

            return Cache
                .GetOrAdd(src, () => Expression
                    .Lambda<Func<object>>(Expression.New(src.GetConstructor(Type.EmptyTypes) ?? throw new MissingMethodException(src.Name, "Ctor(EmptyTypes)")))
                    .Compile())
                .Invoke();
        }

        public static bool HasOwnMethod(this Type src, string name, params Type[] args) => src.GetMethod(name, args)?.DeclaringType == src;

        public static bool IsList(this Type src) => src.IsGenericType && src.GetGenericTypeDefinition() == typeof(List<>);
    }
}