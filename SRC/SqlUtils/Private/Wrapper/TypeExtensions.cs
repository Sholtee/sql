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

using static System.Diagnostics.Debug;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;
    using Properties;

    internal static partial class TypeExtensions
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public static IReadOnlyList<WrappedSelection> GetWrappedSelections(this Type databaseEntityOrView)
        {
            Assert(databaseEntityOrView.IsDatabaseEntityOrView());

            return Cache.GetOrAdd
            (
                databaseEntityOrView,
                () => databaseEntityOrView
                    .GetProperties(BINDING_FLAGS)
                    .Where(PropertyInfoExtensions.IsWrapped)
                    .Select(prop => new WrappedSelection(prop))
                    .ToArray()
            );
        }

        public static bool IsWrapped(this Type view) => view.IsDatabaseEntityOrView() && view.GetWrappedSelections().Any();

        public static IReadOnlyList<ColumnSelection> GetColumnSelections(this Type databaseEntityOrView) => Cache.GetOrAdd(databaseEntityOrView, () => 
        {
            Assert(databaseEntityOrView.IsDatabaseEntityOrView());

            Type? @base = databaseEntityOrView.GetBaseDataType();

            return 
            (
                from prop in databaseEntityOrView.GetProperties(BINDING_FLAGS)
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
                    viewProperty: prop,
                    kind: attr != null ? SelectionKind.Explicit : SelectionKind.Implicit,
                    reason: attr ?? new BelongsToAttribute(@base!)
                )
            ).ToArray();
        });

        public static IReadOnlyList<ColumnSelection> ExtractColumnSelections(this Type databaseEntityOrView) => Cache.GetOrAdd(databaseEntityOrView, () =>
        {
            Assert(databaseEntityOrView.IsDatabaseEntityOrView());

            ColumnSelection[] result = databaseEntityOrView
                .GetColumnSelections()
                .Concat(databaseEntityOrView
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
                group  sel by sel.ViewProperty.Name into grp
                where  grp.Count() > 1
                select grp.Key
            ).ToArray();
                
            if (collisions.Any())
            {
                var ex = new InvalidOperationException(Resources.PROPERTY_NAME_COLLISION);
                ex.Data[nameof(collisions)] = collisions;
                throw ex;
            }

            return result;
        });

        public static Type? GetBaseDataType(this Type databaseEntityOrView)
        {
            while (databaseEntityOrView != null && !Config.Instance.IsDataTable(databaseEntityOrView))
            {
                databaseEntityOrView = databaseEntityOrView.BaseType;
            }
            return databaseEntityOrView;
        }

        public static Type GetEffectiveType(this Type view) 
        {
            Assert(view.IsDatabaseEntityOrView());

            string? mapFromProperty = view.GetCustomAttribute<MapFromAttribute>(inherit: false)?.Property;

            if (mapFromProperty != null) return
            (
                view.GetProperty(mapFromProperty, BINDING_FLAGS) ?? throw new MissingMemberException(view.Name, mapFromProperty)
            ).PropertyType;

            return view;
        }

        public static bool IsValueTypeOrString(this Type src) => src.IsValueType || src == typeof(string);

        public static bool IsDatabaseEntityOrView(this Type type) => type.IsClass && (type.GetCustomAttribute<ViewAttribute>(inherit: false) ?? (object?) type.GetBaseDataType()) != null;

        public static object GetDefaultValue(this Type src) => Cache .GetOrAdd(src, () => Expression
            .Lambda<Func<object>>(Expression.Convert(Expression.Default(src), typeof(object)))
            .Compile()
            .Invoke());

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