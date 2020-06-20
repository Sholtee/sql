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
        #region Instance members
        private Type ViewType { get; }
        private Type UnwrappedType { get; }
        private MappingContext Mappers { get; }

        private Wrapper(Type viewType, Type unwrappedType)
        {
            //
            // Hogy a Wrap() metodus biztosan helyesen csoportositson, minden nezetben kell legyen PK
            //

            viewType.GetPrimaryKey(); // validal

            ViewType      = viewType;
            UnwrappedType = unwrappedType;
            Mappers       = MappingContext.Create(unwrappedType, viewType, new Mapper());
        }

        private void AssignWrappedProperties(object target, IEnumerable group)
        {
            foreach (WrappedSelection sel in ViewType.GetWrappedSelections())
            {
                sel.ViewProperty.FastSetValue(target, new Wrapper(sel.UnderlyingType, UnwrappedType).Wrap(group, sel.IsList));          
            }
        }

        private object Wrap(IEnumerable unwrappedObjects, bool wrapToList)
        {
            IList lst = (IList) typeof(List<>).MakeInstance(ViewType.GetEffectiveType());

            //
            // A kicsomagolt entitasokat csoportositjuk az eredeti nezet NEM listatulajdosagai szerint. Az igy
            // kapott egyes csoportok mar egy-egy nezet peldanyhoz tartoznak (csak a csomagolt tulajdonsagaik
            // ertekeben ternek el).
            //

            foreach (IGrouping<object, object> group in unwrappedObjects.Cast<object>().GroupBy(Mappers.MapToKey, ValueComparer.Instance))
            {
                //
                // Ha az entitas ures (LEFT JOIN miatt kaptuk vissza) akkor nem vesszuk fel.
                //   - Ne a "view" oljektumon vizsgaljuk mert az a mappolas miatt elterhet
                //   - Ne "=="-el vizsgaljunk h mukodjunk ertek tipusokra is
                //

                PropertyInfo pk = group.Key.GetType().GetPrimaryKey();

                if (ValueComparer.Instance.Equals(pk.FastGetValue(group.Key), pk.PropertyType.GetDefaultValue()))
                    continue;

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
                    throw new InvalidOperationException(Resources.AMBIGUOUS_RESULT);

                return lst[0];
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