/********************************************************************************
* TypeExtensions.cs                                                             *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
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

        public static IReadOnlyList<WrappedSelection> GetWrappedSelections(this Type src) => Cache.GetOrAdd(src, () => src
            .GetProperties(BINDING_FLAGS)
            .Where(Config.Instance.IsWrapped)
            .Select(prop => new WrappedSelection(prop))
            .ToArray());

        public static bool IsWrapped(this Type src) => src.GetWrappedSelections().Any();

        public static IReadOnlyList<ColumnSelection> GetColumnSelections(this Type src) => Cache.GetOrAdd(src, () => 
        {
            Type baseOrmType = src.GetBaseOrmType();

            return 
            (
                from prop in src.GetProperties(BINDING_FLAGS)
                let attr = prop.GetCustomAttribute<ColumnSelectionAttribute>()

                //
                // Ha a nezet egy mar meglevo ORM entitas leszarmazottja akkor azon property-ket
                // is kivalasztjuk melyek az os (ORM) entitashoz tartoznak.
                //

                where attr != null || (baseOrmType != null && !Config.Instance.IsIgnored(prop) && prop.DeclaringType.IsAssignableFrom(baseOrmType))
                select new ColumnSelection
                (
                    column: prop,
                    kind: attr != null ? SelectionKind.Explicit : SelectionKind.Implicit,
                    reason: attr ?? new BelongsToAttribute(baseOrmType)
                )
            ).ToArray();
        });

        public static IReadOnlyList<ColumnSelection> ExtractColumnSelections(this Type src) => Cache.GetOrAdd(src, () =>
        {
            ColumnSelection[] result = src
                .GetColumnSelections()
                .Concat(src
                    .GetWrappedSelections()
                    .SelectMany(sel => sel.UnderlyingType.ExtractColumnSelections()))
                .ToArray();

            //
            //      class A {int Foo;}
            //      class B {int Foo;}
            //      class C
            //      {
            //        A A;
            //        B B;
            //      }
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

        public static Type GetBaseOrmType(this Type entityType)
        {
            while (entityType != null && !Config.Instance.IsDataTable(entityType))
            {
                entityType = entityType.BaseType;
            }
            return entityType!;
        }

        public static object GetDefaultValue(this Type src) => Cache
            .GetOrAdd(src, () => Expression
                .Lambda<Func<object>>(Expression.Convert(Expression.Default(src), typeof(object)))
                .Compile())
            .Invoke();

        public static PropertyInfo? GetEmptyListMarker(this Type src) => Cache.GetOrAdd(src, () =>
        {
            try
            {
                return src
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
    }
}