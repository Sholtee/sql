/********************************************************************************
*  Wrapper{TView}.cs                                                            *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Primitives.Patterns;
    using Properties;

    internal class Wrapper<TView>: Singleton<Wrapper<TView>>
    {
        private Func<IList, List<TView>> Core { get; }

        public Wrapper()
        {
            MethodInfo concreteWrapper = typeof(Wrapper<,>)
                .MakeGenericType(typeof(TView), UnwrappedView<TView>.Type)
                .GetMethod(nameof(Wrapper<object, object>.WrapToTypedList), BindingFlags.Public | BindingFlags.Static);

            ParameterExpression sourceObjects = Expression.Parameter(typeof(IList), nameof(sourceObjects));

            Core = Expression.Lambda<Func<IList, List<TView>>>
            (
                Expression.Call
                (
                    null,
                    concreteWrapper,
                    Expression.Convert(sourceObjects, typeof(IEnumerable<>).MakeGenericType(UnwrappedView<TView>.Type))
                ),
                sourceObjects
            ).Compile();
        }

        public static List<TView> Wrap(IList sourceObjects)
        {
            Type
                sourceListType = sourceObjects.GetType(),
                unwrappedType  = UnwrappedView<TView>.Type;

            if (!sourceListType.IsList())
            {
                throw new ArgumentException(Resources.NOT_A_LIST, nameof(sourceObjects)); ;
            }

            if (sourceListType.GetGenericArguments().Single() != unwrappedType)
            {
                throw new ArgumentException(Resources.INCOMPATIBLE_LIST, nameof(sourceObjects));
            }

            return Instance.Core.Invoke(sourceObjects);
        }
    }
}