/********************************************************************************
*  Wrapper.cs                                                                   *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Properties;

    internal class Wrapper
    {
        #region Instance
        private Type ViewType { get; }
        private Type UnwrappedType { get; }
        private MapperContext Mappers{ get; }

        private Wrapper(Type viewType, Type unwrappedType)
        {
            ViewType      = viewType;
            UnwrappedType = unwrappedType;
            Mappers       = MapperContext.Create(unwrappedType, viewType);
        }

        private void AssignWrappedProperties(object target, IEnumerable group)
        {
            foreach (WrappedSelection sel in ViewType.GetWrappedSelections())
            {
                sel.Info.FastSetValue(target, new Wrapper(sel.UnderlyingType, UnwrappedType).Wrap(group, sel.IsList));          
            }
        }

        private object Wrap(IEnumerable unwrappedObjects, bool wrapToList)
        {
            IList lst = (IList) typeof(List<>).MakeInstance(ViewType);

            //
            // A kicsomagolt entitasokat csoportositjuk az eredeti nezet NEM listatulajdosagai szerint. Az igy
            // kapott egyes csoportok mar egy-egy nezet peldanyhoz tartoznak (csak a csomagolt tulajdonsagaik
            // ertekeben ternek el).
            //

            foreach (IGrouping<object, object> group in unwrappedObjects.Cast<object>().GroupBy(Mappers.MapToKey, ValueComparer.Instance))
            {
                //
                // A csoport kulcsa megadja az aktualis nezet peldany nem lista tulajdonsagait -> tolajdonsagok masolasa.
                //

                object view = Mappers.MapToView(group.Key);

                //
                // Az egyes listatulajdonsagok feltoltesehez rekurzivan hivjuk sajat magunkat a lista
                // tipusa szerint.
                //

                AssignWrappedProperties(view, group);

                lst.Add(view);
            }

            if (!wrapToList) 
            {
                if (lst.Count != 1)
                    throw new InvalidOperationException(); // TODO

                return lst[0];
            }

            if (lst.Count == 1)
            {
                //
                // Ha a lista 1 elemu es a lista tulajdonsag rendelkezik EmptyListMarker-rel akkor meg ellenoriznunk 
                // kell azt is h nem csak a LEFTJOIN miatt van e egy elem a listaban.
                //

                PropertyInfo? marker = ViewType.GetEmptyListMarker();

                if (marker != null && marker.FastGetValue(lst[0]) == marker.PropertyType.GetDefaultValue())
                {
                    lst.Clear();
                }
            }

            return lst;
        }
        #endregion

        public static List<TView> Wrap<TView>(IList sourceObjects)
        {
            Type
                sourceListType = sourceObjects.GetType(),
                unwrappedType  = Unwrapped<TView>.Type;

            if (!sourceListType.IsGenericType || sourceListType.GetGenericTypeDefinition() != typeof(List<>))
            {
                throw new ArgumentException(Resources.NOT_A_LIST, nameof(sourceObjects)); ;
            }

            if (sourceListType.GetGenericArguments().Single() != unwrappedType)
            {
                throw new ArgumentException(Resources.INCOMPATIBLE_LIST, nameof(sourceObjects));
            }

            return (List<TView>) new Wrapper(typeof(TView), unwrappedType).Wrap(sourceObjects, true);
        }
    }
}