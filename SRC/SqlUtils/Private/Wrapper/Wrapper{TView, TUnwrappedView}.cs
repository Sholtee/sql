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

    internal class Wrapper<TView, TUnwrappedView>: Singleton<Wrapper<TView, TUnwrappedView>>
    {
        private Func<IEnumerable<TUnwrappedView>, List<TView>> Core { get; }

        public Wrapper()
        {
            //
            // Hogy a Wrap() metodus biztosan helyesen csoportositson, minden nezetben kell legyen PK
            //

            typeof(TView).GetPrimaryKey(); // validal

            ParameterExpression unwrappedObjects = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(typeof(TUnwrappedView)), nameof(unwrappedObjects));

            Expression<Func<IEnumerable<TUnwrappedView>, List<TView>>> coreExpr = Expression.Lambda<Func<IEnumerable<TUnwrappedView>, List<TView>>>(GenerateBody(unwrappedObjects), unwrappedObjects);
            Core = coreExpr.Compile();
        }

        public static List<TView> WrapToList(IEnumerable<TUnwrappedView> unwrappedViews) => Instance.Core.Invoke(unwrappedViews);

        public static TView? WrapToView(IEnumerable<TUnwrappedView> unwrappedViews) 
        {
            IReadOnlyList<TView> lst = Instance.Core.Invoke(unwrappedViews);

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
                // var lst = new List<View>()
                //

                Expression.Assign(lst, Expression.New(lst.Type)),

                //
                // foreach (IGrouping<GropKey, UnwrappedView> group in unwrappedObjects.GroupBy(Mapper<UnwrappedView, GroupKey>.Map))
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
                    // View view = Mapper<GroupKey, View>.Map(group.Key);
                    //

                    MethodInfo mapToView = typeof(Mapper<,>).MakeGenericType(groupKey, view.GetEffectiveType()).GetMethod(nameof(Mapper<object, object>.Map));
                    yield return Expression.Assign
                    (
                        viewVar, 
                        Expression.Call(null, mapToView, key)
                    );

                    //
                    // view.ViewListA = Wrapper<ViewListAType, UnwrappedView>.WrapToList(group);
                    // view.ViewListB = Wrapper<ViewListBType, UnwrappedView>.WrapToList(group);
                    // view.View = Wrapper<View, UnwrappedView>.WrapToView(group);
                    //

                    foreach (WrappedSelection sel in view.GetWrappedSelections())
                    {
                        Type internalWrapper = typeof(Wrapper<,>).MakeGenericType(sel.UnderlyingType, unwrappedView);

                        MethodInfo wrap = sel.IsList
                            ? internalWrapper.GetMethod(nameof(WrapToList), BindingFlags.Public | BindingFlags.Static)
                            : internalWrapper.GetMethod(nameof(WrapToView), BindingFlags.Public | BindingFlags.Static);

                        yield return Expression.Assign
                        (
                            Expression.Property(viewVar, sel.ViewProperty),
                            Expression.Call(null, wrap, group)
                        );
                    }

                    //
                    // lst.Add(view);
                    //

                    MethodInfo lstAdd = lst.Type.GetMethod(nameof(List<object>.Add));
                    yield return Expression.Call(lst, lstAdd, viewVar);
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
                enumeratorAssign  = Expression.Assign(enumerator, getEnumeratorCall),
                moveNextCall      = Expression.Call(enumerator,  typeof(IEnumerator).GetMethod(nameof(IEnumerator.MoveNext)));

            LabelTarget
                continueLabel = Expression.Label("continue"),
                breakLabel = Expression.Label("break");

            return Expression.Block
            (
                variables: new[] { enumerator },
                enumeratorAssign,
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
