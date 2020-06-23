/********************************************************************************
*  MappingContext.cs                                                            *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    internal sealed class MappingContext
    {
        public Func<object?, object?> MapToKey { get; }

        public Func<object?, object?> MapToView { get; }

        public static MappingContext Create(Type unwrappedType, Type viewType) => Cache.GetOrAdd((unwrappedType, viewType), () => new MappingContext(unwrappedType, viewType));

        #region Private stuffs  
        private MappingContext(Type unwrappedType, Type viewType)
        {
            Debug.WriteLine($"Creating mapping context for {(unwrappedType, viewType)}");

            //
            // Az eredeti nezet nem lista property-eibol letrehozunk egy kulcs tipust.
            // Ez lesz a kulcs tipusa a .GroupBy(x => new Kulcs())-ban.
            //

            Type keyType = DefineKey();

            //
            // A mappolas ami kicsomagolt nezet egy peldanyat a konkret kulcsra szukiti:
            //
            //    nagyAdat => nagyAdat.MapTo<Kulcs>()
            //

            MapToKey = Mapper.Create(unwrappedType, keyType);

            //
            // A mappolas ami a kulcs egy peldanyat visszamappolja a nezet entitasba ami alapjan a kulcs
            // keszult (magyaran feltolti az eredeti nezet NEM lista tulajdonsagait).
            //

            MapToView = Mapper.Create(keyType, viewType.GetEffectiveType());

            Type DefineKey() => ViewFactory.CreateView
            (
                new MemberDefinition
                (
                    $"{unwrappedType.FullName}_{viewType.FullName}_Key",
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

            CustomAttributeBuilder[] CopyAttributes(MemberInfo member) => member
                .GetCustomAttributes()
                .OfType<IBuildableAttribute>()
                .Select(attr => CustomAttributeBuilderFactory.CreateFrom(attr))
                .ToArray();
        }
        #endregion      
    }
}