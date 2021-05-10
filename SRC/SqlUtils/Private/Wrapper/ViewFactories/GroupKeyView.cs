/********************************************************************************
*  GroupKeyView.cs                                                              *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;

    //
    // .GropBy(nagyAdat => nagyAdat.MapTo<Kulcs>())
    // .Select(x => x.Key.MapTo<View>())
    //

    internal sealed class GroupKeyView
    {
        public static Type Create(Type unwrappedType, Type viewType) => Cache.GetOrAdd((unwrappedType, viewType), () =>
        {
            return new ViewFactory
            (
                new MemberDefinition
                (
                    $"{unwrappedType}_{viewType}_Key",
                    viewType.GetQueryBase(),
                    CopyAttributes(viewType)
                ),
                GetKeyMembers()
            )
            .CreateType();

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

            CustomAttributeBuilder[] CopyAttributes(MemberInfo member) => member
                .GetCustomAttributes()
                .OfType<IBuildableAttribute>()
                .Select(attr => CustomAttributeBuilderFactory.CreateFrom(attr))
                .ToArray();
        }, nameof(GroupKeyView));    
    }
}