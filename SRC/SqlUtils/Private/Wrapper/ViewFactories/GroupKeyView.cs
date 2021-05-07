/********************************************************************************
*  GroupKeyView.cs                                                              *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;

namespace Solti.Utils.SQL.Internals
{
    using Primitives;

    //
    // .GropBy(nagyAdat => nagyAdat.MapTo<Kulcs>())
    // .Select(x => x.Key.MapTo<View>())
    //

    internal sealed class GroupKeyView: ViewFactory
    {
        public static Type Create(Type unwrappedType, Type viewType) => Cache.GetOrAdd((unwrappedType, viewType), () =>
        {
            return Create
            (
                new MemberDefinition
                (
                    $"{unwrappedType}_{viewType}_Key",
                    viewType.GetQueryBase(),
                    CopyAttributes(viewType)
                ),
                GetKeyMembers()
            );

            IEnumerable<MemberDefinition> GetKeyMembers()
            {
                IReadOnlyList<ColumnSelection> effectiveColumns = unwrappedType.GetColumnSelections();

                //
                // Vesszuk az eredeti nezet NEM lista tulajdonsagait
                //

                foreach (ColumnSelection column in viewType.GetColumnSelections())
                {
                    //
                    // Ha kicsomagolas soran a tulajdonsag at lett nevezve, akkor azt hasznaljuk.
                    //

                    ColumnSelection effectiveColumn = effectiveColumns.SingleOrDefault(ec => ec.ViewProperty.IsRedirectedTo(column.ViewProperty))
                        ?? effectiveColumns.Single(ec => ec.ViewProperty.CanBeMappedIn(column.ViewProperty));

                    yield return new MemberDefinition
                    (
                        effectiveColumn.ViewProperty.Name,
                        effectiveColumn.ViewProperty.PropertyType,
                        CopyAttributes(effectiveColumn.ViewProperty)
                    );
                }
            }
        }, nameof(GroupKeyView));    
    }
}