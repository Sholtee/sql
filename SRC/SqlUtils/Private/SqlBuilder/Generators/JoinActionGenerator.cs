/********************************************************************************
*  JoinActionGenerator.cs                                                       *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using static System.Diagnostics.Debug;

namespace Solti.Utils.SQL.Internals
{
    internal sealed class JoinActionGenerator<TView> : ActionGenerator<TView>
    {
        public Edge[] CustomEdges { get; }

        public JoinActionGenerator(params Edge[] customEdges) => CustomEdges = customEdges;

        protected override IEnumerable<MethodCallExpression> Generate(ParameterExpression bldr)
        {
            Type fromTable = typeof(TView).GetQueryBase();

            ISet<Type> joinedTables = new HashSet<Type>(new[]
            {
                fromTable
            });

            foreach (var toTable in Selections
                .GroupBy(sel => sel.Reason.OrmType)
                .Select(grp => new 
                {
                    Type = grp.Key, 
                    Required = grp.Any(sel => sel.Reason.Required)
                }))
            {
                //
                // A legrovidebb ut megkeresese a kiindulo tablabol az adott (property-k feltoltesehez szukseges)
                // tablaig.
                //

                foreach (Edge edge in EdgeOperations.ShortestPath(fromTable, toTable.Type, CustomEdges))
                {
                    PropertyInfo joinTableCol, toTableCol;

                    if (joinedTables.Add(edge.SourceTable))
                    {
                        joinTableCol = edge.SourceProperty;          
                        toTableCol = edge.DestinationProperty;

                        Assert(joinedTables.Contains(edge.DestinationTable));
                    }
                    else if (joinedTables.Add(edge.DestinationTable))
                    {
                        joinTableCol = edge.DestinationProperty;
                        toTableCol = edge.SourceProperty;

                        Assert(joinedTables.Contains(edge.SourceTable));
                    }

                    //
                    // Ha az utvonal minden elemet korabban JOIN-oltuk mar akkor nincs dolgunk.
                    //

                    else continue;

                    //
                    // Akcio letrehozasa ami a letrehozott metodust hivja tetszoleges builder-en
                    //

                    yield return Expression.Call(
                        bldr,
                        toTable.Required ? Join.Inner : Join.Left,
                        Expression.Constant(toTableCol),
                        Expression.Constant(joinTableCol));
                }
            }
        }
    }
}