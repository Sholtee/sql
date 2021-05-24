/********************************************************************************
*  GroupKeyView{TView, TUnwrappedView}.cs                                       *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "This class is passed as a type parameter")]
    internal sealed class GroupKeyView<TView, TUnwrappedView>: ViewFactory<GroupKeyView<TView, TUnwrappedView>>
    {
        protected override Type CreateView()
        {
            Type
                view = typeof(TView),
                unwrapped = typeof(TUnwrappedView);

            return CreateView
            (
                new MemberDefinition
                (
                    $"{unwrapped}_{view}_Key",
                    view.GetQueryBase(),
                    GetClassAttributes().ToArray()
                ),
                GetKeyMembers().ToArray()
            );
        }

        private static IEnumerable<CustomAttributeBuilder> GetClassAttributes() 
        {
            Type
                view = typeof(TView),
                unwrapped = typeof(TUnwrappedView);

            //
            // Ha az eredeti nezet rendelkezik MapFrom attributummal akkor azt masolni kell, viszont elofordulhat h
            // az oszlop a kicsomagolas soran at lett nevezve ezert kicsit trukkos
            //

            MapFromAttribute? mapFrom = view.GetCustomAttribute<MapFromAttribute>();

            if (mapFrom is not null)
            {
                PropertyInfo mapFromProp = view.GetProperty(mapFrom.Property) ?? throw new MissingMemberException(view.Name, mapFrom.Property);
                mapFromProp = unwrapped
                    .GetProperties()
                    .Single(prop => prop.CanBeMappedIn(mapFromProp));

                yield return CustomAttributeBuilderFactory.CreateFrom<MapFromAttribute>(new[] { typeof(string) }, new object?[] { mapFromProp.Name });
            }
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
                    effectiveColumn.ViewProperty
                        .GetCustomAttributes()
                        .OfType<IBuildableAttribute>()
                        .Select(attr => CustomAttributeBuilderFactory.CreateFrom(attr))
                        .ToArray()
                );
            }
        }
    }
}