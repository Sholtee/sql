/********************************************************************************
*  GroupKeyView{TView, TUnwrappedView}.cs                                       *
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

    //
    // .GropBy(nagyAdat => nagyAdat.MapTo<Kulcs>())
    // .Select(x => x.Key.MapTo<View>())
    //

    internal sealed class GroupKeyView<TView, TUnwrappedView>: ViewFactory<GroupKeyView<TView, TUnwrappedView>>
    {
        protected override Type CreateView()
        {
            Type
                viewType = typeof(TView),
                unwrappedType = typeof(TUnwrappedView);

            return CreateView
            (
                new MemberDefinition
                (
                    $"{unwrappedType}_{viewType}_Key",
                    viewType.GetQueryBase(),
                    CopyAttributes(viewType)
                ),
                GetKeyMembers()
            );
        }

        private static IEnumerable<MemberDefinition> GetKeyMembers()
        {
            IReadOnlyList<ColumnSelection> effectiveColumns = typeof(TUnwrappedView).GetColumnSelections();

            //
            // Vesszuk az eredeti nezet NEM lista tulajdonsagait
            //

            foreach (ColumnSelection column in typeof(TView).GetColumnSelections())
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

       private static CustomAttributeBuilder[] CopyAttributes(MemberInfo member) => member
            .GetCustomAttributes()
            .OfType<IBuildableAttribute>()
            .Select(attr => CustomAttributeBuilderFactory.CreateFrom(attr))
            .ToArray();
    }
}