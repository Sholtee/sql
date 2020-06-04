/********************************************************************************
*  MapperContext.cs                                                             *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    using Primitives;

    internal sealed class MapperContext
    {
        public Func<object, object> MapToKey { get; private set; }
        public Func<object, object> MapToView { get; private set; }

        #region Private stuffs
        private MapperContext(Func<object, object> mapToKey, Func<object, object> mapToView)
        {
            MapToKey = mapToKey;
            MapToView = mapToView;
        }

        private static Type DefineKey(Type viewType) => Cache.GetOrAdd(viewType, () =>
        {
            TypeBuilder tb = MyTypeBuilder.Create($"{viewType.FullName}_Key");

            foreach (ColumnSelection sel in viewType.GetColumnSelections())
            {
                tb.AddProperty(sel.Column);
            }

            return tb.CreateTypeInfo()!.AsType();
        });

        #endregion

        #region Static stuffs
        public static IMapper Mapper = new Mapper(); // tesztekben felulirhato

        public static MapperContext Create(Type unwrappedType, Type viewType) => Cache.GetOrAdd((unwrappedType, viewType), () =>
        {
            //
            // Az eredeti nezet nem lista property-eibol letrehozunk egy kulcs tipust.
            // Ez lesz a kulcs tipusa a .GroupBy(x => new Kulcs())-ban.
            //

            Type keyType = DefineKey(viewType);

            //
            // A mappolas ami kicsomagolt nezet egy peldanyat a konkret kulcsra szukiti:
            //
            //    nagyAdat => nagyAdat.MapTo<Kulcs>()
            //

            Mapper.RegisterMapping(unwrappedType, keyType);

            //
            // A mappolas ami a kulcs egy peldanyat visszamappolja a nezet enitasba ami alapjan a kulcs
            // keszult (magyaran feltolti az eredeti nezet NEM lista tulajdonsagait).
            //

            Mapper.RegisterMapping(keyType, viewType);

            //
            // .GropBy(nagyAdat => nagyAdat.MapTo<Kulcs>())
            // .Select(x => x.Key.MapTo<View>())
            //

            return new MapperContext
            (
                mapToKey:  src => Mapper.MapTo(unwrappedType, keyType, src)!,
                mapToView: src => Mapper.MapTo(keyType, viewType, src)!
            );
        });
        #endregion
    }
}