/********************************************************************************
*  MappingContext.cs                                                            *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Internals
{
    using Primitives;

    internal sealed class MappingContext
    {
        public Func<object?, object?> MapToKey { get; }
        public Func<object?, object?> MapToView { get; }

        #region Private stuffs
        private MappingContext(Func<object?, object?> mapToKey, Func<object?, object?> mapToView)
        {
            MapToKey = mapToKey;
            MapToView = mapToView;
        }

        private static Type DefineKey(Type viewType) => Cache.GetOrAdd
        (
            viewType, 
            () => viewType.IsWrapped() 
                ? ViewFactory.CreateView
                (
                    new MemberDefinition
                    (
                        $"{viewType.FullName}_Key", 
                        viewType.GetQueryBase()
                    ),
                    viewType.GetColumnSelections()
                )
                : viewType
        );
        #endregion

        #region Static stuffs
        public static MappingContext Create(Type unwrappedType, Type viewType) => Cache.GetOrAdd((unwrappedType, viewType), () =>
        {
            //
            // Az eredeti nezet nem lista property-eibol letrehozunk egy kulcs tipust.
            // Ez lesz a kulcs tipusa a .GroupBy(x => new Kulcs())-ban.
            //

            Type keyType = DefineKey(viewType);

            //
            // .GropBy(nagyAdat => nagyAdat.MapTo<Kulcs>())
            // .Select(x => x.Key.MapTo<View>())
            //

            return new MappingContext
            (
                //
                // A mappolas ami kicsomagolt nezet egy peldanyat a konkret kulcsra szukiti:
                //
                //    nagyAdat => nagyAdat.MapTo<Kulcs>()
                //

                mapToKey:  Mapper.Create(unwrappedType, keyType),

                //
                // A mappolas ami a kulcs egy peldanyat visszamappolja a nezet enitasba ami alapjan a kulcs
                // keszult (magyaran feltolti az eredeti nezet NEM lista tulajdonsagait).
                //

                mapToView: Mapper.Create(keyType, viewType.GetEffectiveType())
            );
        });
        #endregion
    }
}