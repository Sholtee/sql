﻿/********************************************************************************
*  EdgeOperations.cs                                                            *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Primitives;
    using Properties;

    internal static class EdgeOperations
    {
        #region Private stuffs
        private const BindingFlags BINDING_FLAGS = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        internal static IReadOnlyList<Edge> GetEdgesFrom(Type type) => 
        //
        // Nem kell gyorsitotarazni magaban ugy sincs hasznalva
        //

        (
            from   prop in type.GetProperties(BINDING_FLAGS)
            let    referenced = Config.Instance.GetReferencedType(prop)
            where  referenced != null
            select new Edge(prop, referenced.GetPrimaryKey())
        ).ToArray();

        internal static IReadOnlyList<Edge> GetEdgesTo(Type type) =>
        //
        // Nem kell gyorsitotarazni magaban ugy sincs hasznalva
        //

        (
            from   table in Config.KnownTables
            let    edge = GetEdgesFrom(table).SingleOrDefault(e => e.DestinationTable == type)
            where  edge != null
            select edge
        ).ToArray();

        internal static IReadOnlyList<Edge> GetEdges(Type type) => Cache.GetOrAdd(type, () => (IReadOnlyList<Edge>) GetEdgesFrom(type).Concat(GetEdgesTo(type)).ToArray());

        private static IReadOnlyList<Edge>? ShortestPath(Type src, Type dst, IReadOnlyList<Edge> customEdges, IReadOnlyList<Edge> currentPath)
        {
            //
            // Ha az utolso el "dst"-bol vagy "dst"-be mutat, akkor jok vagyunk.
            //

            if (src == dst) return currentPath;

            //
            // Kulonben vesszuk az osszes elet ami az utolso csomopontbol indul ki vagy
            // abba erkezik (kiveve a mar bejart utakhoz tartozoakat).
            //

            IReadOnlyList<Edge>? shortestPath = null;

            foreach (Edge edge in GetEdges(src)
                .Concat
                (
                    //
                    // Az egyedi elek kozul amik a csomoponthoz tartoznak
                    //

                    customEdges.Where(edge => edge.SourceTable == src || edge.DestinationTable == src)
                )
                .Where(edge => !currentPath.Contains(edge)))
            {
                //
                // Ha az adott elen keresztul elerjuk "dst"-t es ez az el meg rovidebb is
                // mint az eddig nyilvantartott, akkor uj utvonalunk van.
                //

                IReadOnlyList<Edge>? newPath = currentPath.Append(edge).ToArray();
                Type newSrc = (edge.SourceTable == src) ? edge.DestinationTable : edge.SourceTable;

                if ((newPath = ShortestPath(newSrc, dst, customEdges, newPath)) != null && (shortestPath == null || newPath.Count < shortestPath.Count))
                {
                    shortestPath = newPath;
                }
            }

            //
            // Itt mar -elmeletileg- a legrovidebb utnak kell lennie (ha van).
            //

            return shortestPath;
        }
        #endregion

        public static IReadOnlyList<Edge> ShortestPath(Type src, Type dst, params Edge[] customEdges)
        {
            IReadOnlyList<Edge>? result = Cache.GetOrAdd(
                (src, dst, ValueComparer.Instance.GetHashCode(customEdges)), 
                () => ShortestPath(src, dst, customEdges, Array.Empty<Edge>()));

            if (result == null)
            {
                var ex = new InvalidOperationException(Resources.NO_SHORTEST_PATH);
                ex.Data[nameof(src)] = src;
                ex.Data[nameof(dst)] = dst;

                throw ex;
            }

            return result;
        }
    }
}