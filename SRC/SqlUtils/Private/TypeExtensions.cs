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

        public static IReadOnlyList<PropertyInfo> GetWrappedSelections(this Type databaseEntityOrView) => Cache.GetOrAdd(databaseEntityOrView, () =>
        {
            Assert(databaseEntityOrView.IsDatabaseEntityOrView());
            return GetWrappedSelections().ToArray();

            IEnumerable<PropertyInfo> GetWrappedSelections()
            {
                foreach (PropertyInfo prop in databaseEntityOrView.GetProperties(BINDING_FLAGS))
                {
                    if (!prop.IsWrapped())
                        continue;

                    if (!prop.GetEffectiveType().IsDatabaseEntityOrView())
                    {
                        var ex = new InvalidOperationException(Resources.CANT_WRAP);
                        ex.Data[nameof(prop)] = prop;
                        throw ex;
                    }

                    yield return prop;
                }
            }
        });

        public static bool IsWrapped(this Type view) => view.IsDatabaseEntityOrView() && view.GetWrappedSelections().Any();

        public static IReadOnlyList<ColumnSelection> GetColumnSelections(this Type databaseEntityOrView) => Cache.GetOrAdd(databaseEntityOrView, () => 
        {
            Assert(databaseEntityOrView.IsDatabaseEntityOrView());

            return 
            (
                from prop in databaseEntityOrView.GetProperties(BINDING_FLAGS)
                where !prop.IsWrapped()
                let sel = prop.AsColumnSelection(true)
                where sel is not null
                select sel
            ).ToArray();
        });

        public static IReadOnlyList<ColumnSelection> GetColumnSelectionsDeep(this Type databaseEntityOrView) => Cache.GetOrAdd(databaseEntityOrView, () =>
        {
            Assert(databaseEntityOrView.IsDatabaseEntityOrView());

            return GetColumnSelectionsDeep(databaseEntityOrView, true).ToArray();

            static IEnumerable<ColumnSelection> GetColumnSelectionsDeep(Type view, bool baseRequired)
            {
                foreach (PropertyInfo prop in view.GetProperties(BINDING_FLAGS))
                {
                    if (prop.IsWrapped())
                    {
                        Type underlyingView = prop.GetEffectiveType();

                        if (!underlyingView.IsDatabaseEntityOrView())
                        {
                            var ex = new InvalidOperationException(Resources.CANT_WRAP);
                            ex.Data[nameof(prop)] = prop;
                            throw ex;
                        }

                        foreach (ColumnSelection sel in GetColumnSelectionsDeep(underlyingView, prop.GetCustomAttribute<WrappedAttribute>()?.Required is not false))
                        {
                            yield return sel;
                        }
                    }
                    else
                    {
                        ColumnSelection? sel = prop.AsColumnSelection(baseRequired);
                        if (sel is not null)
                            yield return sel;
                    }
                }
            }
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
            if (result is null)
            {
                var ex = new InvalidOperationException(Resources.BASE_CANNOT_BE_DETERMINED);
                ex.Data[nameof(viewOrDatabaseEntity)] = viewOrDatabaseEntity;
                throw ex;
            }
            return result;
        }
    }
}