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

using static System.Diagnostics.Debug;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;
    using Properties;

    internal static class TypeExtensions
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

            Type? @base = databaseEntityOrView.GetBaseDataTable();

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

        public static IReadOnlyList<ColumnSelection> GetColumnSelectionsDeep(this Type databaseEntityOrView) => Cache.GetOrAdd(databaseEntityOrView, () =>
        {
            Assert(databaseEntityOrView.IsDatabaseEntityOrView());

            return databaseEntityOrView
                .GetColumnSelections()
                .Concat(databaseEntityOrView
                    .GetWrappedSelections()
                    .SelectMany(sel =>
                    {
                        Assert(sel.UnderlyingType.IsDatabaseEntityOrView());

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

                        return sel.UnderlyingType.GetColumnSelectionsDeep();
                    }))
                .ToArray();
        });

        public static Type? GetBaseDataTable(this Type databaseEntityOrView) => 
            Config.KnownTables.SingleOrDefault(dataTable => dataTable.IsAssignableFrom(databaseEntityOrView));

        public static PropertyInfo? MapFrom(this Type view) 
        {
            string? mapFromProperty = view.GetCustomAttribute<MapFromAttribute>(inherit: false)?.Property;

            return mapFromProperty != null
                ? view.GetProperty(mapFromProperty, BINDING_FLAGS) ?? throw new MissingMemberException(view.Name, mapFromProperty)
                : null;
        }

        public static Type GetEffectiveType(this Type view) => view.MapFrom()?.PropertyType ?? view;

        public static bool IsValueTypeOrString(this Type src) => src.IsValueType || src == typeof(string);

        public static bool IsDatabaseEntityOrView(this Type type) => type.IsClass && (type.GetCustomAttribute<ViewAttribute>(inherit: false) ?? (object?) type.GetBaseDataTable()) != null;

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

        public static bool IsList(this Type src) => src.IsGenericType && src.GetGenericTypeDefinition() == typeof(List<>);

        public static PropertyInfo GetPrimaryKey(this Type viewOrDatabaseEntity) => Cache.GetOrAdd(viewOrDatabaseEntity, () =>
        {
            PropertyInfo? pk;

            //
            // Ha a parameter adattabla v olyan nezet ami adattablabol szarmazik akkor 
            // egyszeruen vissza tudjuk adni az elsodleges kulcsot.
            //

            Type? databaseEntity = viewOrDatabaseEntity.GetBaseDataTable();

            if (databaseEntity != null) pk = databaseEntity
                .GetProperties(BINDING_FLAGS)
                .SingleOrDefault(Config.Instance.IsPrimaryKey);

            //
            // Kulonben megkeressuk azt a nezet tulajdonsagot ami az elsodleges kulcsra hivatkozik
            //

            else
            {
                pk = viewOrDatabaseEntity
                    .GetQueryBase()
                    .GetPrimaryKey();

                pk = viewOrDatabaseEntity
                    .GetColumnSelections()
                    .Where(sel =>
                        //
                        // Aggregatum kivalasztasok nem jatszanak
                        // 

                        sel.Reason is BelongsToAttribute bta && bta.OrmType == pk.DeclaringType && (bta.Column ?? sel.ViewProperty.Name) == pk.Name)
                    .Select(sel => sel.ViewProperty)
                    .SingleOrDefault();
            }

            if (pk == null)
            {
                var ex = new MissingMemberException(Resources.NO_PRIMARY_KEY);
                ex.Data[nameof(viewOrDatabaseEntity)] = viewOrDatabaseEntity;
                throw ex;
            }

            return pk;
        });

        public static Type GetQueryBase(this Type viewOrDatabaseEntity)
        {
            Type? result = viewOrDatabaseEntity.GetCustomAttribute<ViewAttribute>(inherit: false)?.Base ?? viewOrDatabaseEntity.GetBaseDataTable();
            if (result == null)
            {
                var ex = new InvalidOperationException(Resources.BASE_CANNOT_BE_DETERMINED);
                ex.Data[nameof(viewOrDatabaseEntity)] = viewOrDatabaseEntity;
                throw ex;
            }
            return result;
        }
    }
}