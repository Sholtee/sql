/********************************************************************************
*  MappingContext.cs                                                            *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Diagnostics;

namespace Solti.Utils.SQL.Internals
{
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
 
        private MappingContext(Type unwrappedType, Type viewType)
        {
            Debug.WriteLine($"Creating mapping context for {(unwrappedType, viewType)}");

            //
            // Az eredeti nezet nem lista property-eibol letrehozunk egy kulcs tipust.
            // Ez lesz a kulcs tipusa a .GroupBy(x => new Kulcs())-ban.
            //

            Type keyType = GroupKeyView.CreateView(unwrappedType, viewType);

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
        }    
    }
}