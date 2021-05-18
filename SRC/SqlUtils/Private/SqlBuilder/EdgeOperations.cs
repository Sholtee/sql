/********************************************************************************
* EdgeOperations.cs                                                             *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Properties;

    internal static class EdgeOperations
    {
        private static IReadOnlyDictionary<Type, IReadOnlyList<Edge>> GetAllEdges() 
        {
            Dictionary<Type, IReadOnlyList<Edge>> result = new();

            foreach (Type table in Config.KnownTables)
            {
                //
                // Kivalasztjuk a tablabol kiindulo eleket
                //

                IEnumerable<Edge> edgesFrom =
                (
                    from   prop in table.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                    let    referenced = Config.Instance.GetReferencedType(prop)
                    where  referenced is not null && referenced != table
                    select new Edge(prop, referenced.GetPrimaryKey())
                );

                //
                // Mentjuk oket
                //

                GetRelatedList(table).AddRange(edgesFrom);

                //
                // Majd ezen elek cel-tablaihoz is regisztraljuk az eleket.
                //

                foreach (Edge edge in edgesFrom)
                {
                    Debug.Assert(edge.SourceTable == table);
                    GetRelatedList(edge.DestinationTable).Add(edge);
                }
            }

            return result;

            List<Edge> GetRelatedList(Type table)
            {
                if (!result.TryGetValue(table, out IReadOnlyList<Edge> edgesFrom))
                    result.Add(table, edgesFrom = new List<Edge>());

                return (List<Edge>) edgesFrom;
            }
        }


        private static IReadOnlyDictionary<Type, IReadOnlyList<Edge>>? FAllEdges;

#if DEBUG
        public static void Reset() => FAllEdges = null;
#endif

        public static IReadOnlyDictionary<Type, IReadOnlyList<Edge>> AllEdges => FAllEdges ??= GetAllEdges();

        public static IReadOnlyList<Edge> ShortestPath(Type src, Type dst, params Edge[] customEdges)
        {
            IReadOnlyList<Edge>? shortestPath = null;

            Walk(src, dst, ImmutableList<Edge>.Empty);

            if (shortestPath is null)
            {
                var ex = new InvalidOperationException(Resources.NO_SHORTEST_PATH);
                ex.Data[nameof(src)] = src;
                ex.Data[nameof(dst)] = dst;

                throw ex;
            }

            //
            // Itt mar -elmeletileg- a legrovidebb utnak kell lennie (ha van).
            //

            return shortestPath;

            void Walk(Type from, Type to, ImmutableList<Edge> currentPath)
            {
                Debug.WriteLine(string.Join(" ", currentPath));

                //
                // Ha az utolso el "dst"-bol vagy "dst"-be mutat, akkor jok vagyunk.
                //

                if (from == to)
                {
                    //
                    // Meg ellenorizzuk h az uj utvonal rovidebb e az eddig nyilvantartottnal
                    //

                    if (shortestPath is null || currentPath.Count < shortestPath.Count)
                        shortestPath = currentPath;

                    return;
                }

                //
                // Ha van mar lehetseges legrovidebb ut akkor eleg a tobbi utat csak addig bejarni
                // amig azok rovidebbek a lehetseges legrovidebb utnal.
                //

                if (customEdges.Length == shortestPath?.Count)
                    return;

                //
                // Kulonben vesszuk az osszes elet ami az utolso csomopontbol indul ki vagy
                // abba erkezik (kiveve a mar bejart utakhoz tartozoakat).
                //

                if (!AllEdges.TryGetValue(from, out IReadOnlyList<Edge> relatedEdges))
                    relatedEdges = Array.Empty<Edge>();

                foreach (Edge edge in relatedEdges
                    .Concat
                    (
                        //
                        // Az egyedi elek kozul amik a csomoponthoz tartoznak
                        //

                        customEdges.Where(edge => edge.SourceTable == from || edge.DestinationTable == from)
                    )
                    .Where(edge => !currentPath.Contains(edge)))
                {
                    //
                    // Az uj elnel folytatjuk a bejarast
                    //

                    Walk
                    (
                        edge.SourceTable == from
                            ? edge.DestinationTable
                            : edge.SourceTable,
                        to,
                        currentPath.Add(edge)
                    );
                }
            }
        }
    }
}