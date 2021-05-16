/********************************************************************************
*  Wrapper{TView, TUnwrappedView}.cs                                            *
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

    /****************************************************************************************************************************
     * List<TView | TPropertyValue> lst = new List<TView | TPropertyValue>();
     * 
     * //
     * // A kicsomagolt entitasokat csoportositjuk az eredeti nezet NEM listatulajdosagai szerint. Az igy
     * // kapott egyes csoportok mar egy-egy nezet peldanyhoz tartoznak (csak a csomagolt tulajdonsagaik
     * // ertekeben ternek el).
     * //
     * 
     * foreach (IGrouping<TGroupKey, TUnwrappedView> group in unwrappedObjects.GroupBy(Mapper<TUnwrappedView, TGroupKey>.Map))
     * {
     *     //
     *     // Ha az entitas ures (LEFT JOIN miatt kaptuk vissza) akkor nem vesszuk fel.
     *     //
     *     
     *     if (group.Key.PrimaryKey == default)
     *         continue;
     *         
     *     //
     *     // A csoport kulcsa megadja az aktualis nezet peldany nem lista tulajdonsagait -> tolajdonsagok masolasa.
     *     //
     *     
     *     TView | TPropertyValue view = Mapper<TGroupKey, TView | TPropertyValue>.Map(group.Key);
     *     
     *     //
     *     // Az egyes listatulajdonsagok feltoltesehez rekurzivan hivjuk sajat magunkat a lista
     *     // tipusa szerint.
     *     //
     *     
     *     view.ViewList  = (List<TViewA>)      Wrapper<TViewA, TUnwrappedView>.WrapToList(group);
     *     view.ValueList = (List<TValueType>)  Wrapper<TViewB, TUnwrappedView>.WrapToList(group);  // lasd UnwrappedValueTypeView
     *     view.View      =                     Wrapper<TViewC, TUnwrappedView>.WrapToView(group);
     *     
     *     ...
     *     
     *     lst.Add(view);
     * }
     * 
     * return lst;
     ****************************************************************************************************************************/

    internal class Wrapper<TView, TUnwrappedView>: Singleton<Wrapper<TView, TUnwrappedView>>
    {
        private Func<IEnumerable<TUnwrappedView>, IList> Core { get; }

        public Wrapper()
        {
            //
            // Hogy a Wrap() metodus biztosan helyesen csoportositson, minden nezetben kell legyen PK
            //

            typeof(TView).GetPrimaryKey(); // validal

            ParameterExpression unwrappedObjects = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(typeof(TUnwrappedView)), nameof(unwrappedObjects));

            Expression<Func<IEnumerable<TUnwrappedView>, IList>> coreExpr = Expression.Lambda<Func<IEnumerable<TUnwrappedView>, IList>>(GenerateBody(unwrappedObjects), unwrappedObjects);
            Core = coreExpr.Compile();
        }

        public static List<TView> WrapToTypedList(IEnumerable<TUnwrappedView> unwrappedViews) => (List<TView>) WrapToList(unwrappedViews);

        public static IList WrapToList(IEnumerable<TUnwrappedView> unwrappedViews) => Instance.Core.Invoke(unwrappedViews);

        public static TView? WrapToView(IEnumerable<TUnwrappedView> unwrappedViews) 
        {
            IReadOnlyList<TView> lst = WrapToTypedList(unwrappedViews);

            if (lst.Count > 1)
                throw new InvalidOperationException(Resources.AMBIGUOUS_RESULT);

            //
            // Lehet NULL is egy csomagolt tulajdonsag.
            //

            return lst.Count == 0 ? default : lst[0];
        }

        private BlockExpression GenerateBody(ParameterExpression unwrappedObjects)
        {
            Type
                view = typeof(TView), 
                unwrappedView = typeof(TUnwrappedView), 
                groupKey = GroupKeyView.CreateView(unwrappedView, view);

            ParameterExpression lst = Expression.Variable(typeof(List<>).MakeGenericType(view.GetEffectiveType()), nameof(lst));

            MethodInfo
                groupBy = ((Func<IEnumerable<object>, Func<object, object>, IEnumerable<IGrouping<object, object>>>) Enumerable.GroupBy<object, object>)
                    .Method
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(unwrappedView, groupKey),
                mapToKey = typeof(Mapper<,>)
                    .MakeGenericType(unwrappedView, groupKey)
                    .GetMethod(nameof(Mapper<object, object>.Map), BindingFlags.Public | BindingFlags.Static);

            ParameterExpression group = Expression.Variable(typeof(IGrouping<,>).MakeGenericType(groupKey, unwrappedView), nameof(group));

            return Expression.Block
            (
                variables: new[] { lst },

                //
                // var lst = new List<TView>()
                //

                Expression.Assign(lst, Expression.New(lst.Type)),

                //
                // foreach (IGrouping<TGropKey, TUnwrappedView> group in unwrappedObjects.GroupBy(Mapper<TUnwrappedView, TGroupKey>.Map))
                // {
                //   ...
                // }
                //

                GenerateForEach
                (
                    Expression.Call
                    (
                        null,
                        groupBy,
                        unwrappedObjects,
                        Expression.Constant
                        (
                            Delegate.CreateDelegate(typeof(Func<,>).MakeGenericType(unwrappedView, groupKey), mapToKey)
                        )
                    ),
                    group,
                    GenerateForaEachBody
                ),

                //
                // return lst;
                //

                lst
            );

            BlockExpression GenerateForaEachBody(ParameterExpression group, LabelTarget @break, LabelTarget @continue)
            {
                ParameterExpression viewVar = Expression.Parameter(view.GetEffectiveType(), nameof(view));

                return Expression.Block
                (
                    variables: new[] { viewVar },
                    expressions: GetBlockExpressions()
                );

                IEnumerable<Expression> GetBlockExpressions()
                {
                    PropertyInfo pk = groupKey.GetPrimaryKey();

                    MemberExpression key = Expression.Property(group, group.Type.GetProperty(nameof(IGrouping<object, object>.Key)));

                    //
                    // if (group.Key.PrimaryKey == default) continue;
                    //

                    yield return Expression.IfThen
                    (
                        Expression.Equal(Expression.Property(key, pk), Expression.Default(pk.PropertyType)), 
                        Expression.Continue(@continue)
                    );

                    //
                    // TView view = Mapper<TGroupKey, TView>.Map(group.Key);
                    //

                    yield return Expression.Assign
                    (
                        viewVar, 
                        Expression.Call
                        (
                            null, 
                            typeof(Mapper<,>)
                                .MakeGenericType(groupKey, view.GetEffectiveType())
                                .GetMethod(nameof(Mapper<object, object>.Map)),
                            key
                        )
                    );

                    //
                    // view.ViewList  = (List<TViewA>)     Wrapper<TViewA, TUnwrappedView>.WrapToList(group);
                    // view.ValueList = (List<TValueType>) Wrapper<TViewB, TUnwrappedView>.WrapToList(group);
                    // view.View      =                    Wrapper<TViewC, TUnwrappedView>.WrapToView(group);
                    //

                    foreach (WrappedSelection sel in view.GetWrappedSelections())
                    {
                        Type internalWrapper = typeof(Wrapper<,>).MakeGenericType(sel.UnderlyingType, unwrappedView);

                        Expression wrap = sel.IsList
                            ? Expression.Convert
                            (
                                Expression.Call
                                (
                                    null, 
                                    internalWrapper.GetMethod(nameof(WrapToList), BindingFlags.Public | BindingFlags.Static), 
                                    group
                                ),
                                sel.ViewProperty.PropertyType
                            )
                            : Expression.Call
                            (
                                null, 
                                internalWrapper.GetMethod(nameof(WrapToView), BindingFlags.Public | BindingFlags.Static), 
                                group
                            );

                        yield return Expression.Assign
                        (
                            Expression.Property(viewVar, sel.ViewProperty),
                            wrap
                        );
                    }

                    //
                    // lst.Add(view);
                    //

                    yield return Expression.Call
                    (
                        lst, 
                        lst
                            .Type
                            .GetMethod(nameof(List<object>.Add)),
                        viewVar
                    );
                }
            }
        }

        private static BlockExpression GenerateForEach(Expression collection, ParameterExpression loopVar, Func<ParameterExpression, LabelTarget, LabelTarget, Expression> bodyFactory)
        {
            Type
                enumerableType = typeof(IEnumerable<>).MakeGenericType(loopVar.Type),
                enumeratorType = typeof(IEnumerator<>).MakeGenericType(loopVar.Type);

            ParameterExpression
                enumerator = Expression.Variable(enumeratorType, "enumerator");

            Expression
                getEnumeratorCall = Expression.Call(collection, enumerableType.GetMethod(nameof(IEnumerable.GetEnumerator))),
                moveNextCall      = Expression.Call(enumerator,  typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext)));

            LabelTarget
                continueLabel = Expression.Label("continue"),
                breakLabel = Expression.Label("break");

            return Expression.Block
            (
                variables: new[] { enumerator },
                Expression.Assign(enumerator, getEnumeratorCall),
                Expression.Loop
                (
                    Expression.IfThenElse
                    (
                        Expression.Equal(moveNextCall, Expression.Constant(true)),
                        Expression.Block
                        (
                            new[] { loopVar },
                            Expression.Assign(loopVar, Expression.Property(enumerator, "Current")),
                            bodyFactory(loopVar, breakLabel, continueLabel)
                        ),
                        Expression.Break(breakLabel)
                    ),
                    breakLabel,
                    continueLabel
                )
            );
        }
    }
}
