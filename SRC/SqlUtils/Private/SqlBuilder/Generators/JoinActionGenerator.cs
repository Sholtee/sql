/********************************************************************************
*  JoinActionGenerator.cs                                                       *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Solti.Utils.SQL.Internals
{
    internal sealed class JoinActionGenerator<TView, TBasePoco> : ActionGenerator<TView>
    {
        public Edge[] CustomEdges { get; }

        public JoinActionGenerator(params Edge[] customEdges) => CustomEdges = customEdges;

        protected override IEnumerable<MethodCallExpression> Generate(ParameterExpression bldr)
        {
            ISet<Type> joinedTables = new HashSet<Type>(new[]
            {
                typeof(TBasePoco)
            });

            foreach (var table in Selections
                .GroupBy(sel => sel.Reason.OrmType)
                .Select(grp => new {Type = grp.Key, Required = grp.Any(sel => sel.Reason.Required)}))
            {
                //
                // A legrovidebb ut megkeresese a kiindulo tablabol az adott (property-k feltoltesehez szukseges)
                // tablaig. Ha az utvonal egy reszet mar korabban bejartuk (JOIN-olva lett) akkor azt
                // mar kihagyhatjuk.
                //

                foreach (Edge edge in EdgeOperations
                    .ShortestPath(typeof(TBasePoco), table.Type, CustomEdges)
                    .Where(edge => joinedTables.Add(edge.SourceTable) || joinedTables.Add(edge.DestinationTable)))
                {
                    //
                    // Akcio letrehozasa ami a letrehozott metodust hivja tetszoleges builder-en
                    //

                    yield return Expression.Call(
                        bldr,
                        table.Required ? QueryMethods.InnerJoin : QueryMethods.LeftJoin,
                        Expression.Constant(edge.SourceProperty),
                        Expression.Constant(edge.DestinationProperty));
                }
            }
        }
    }
}