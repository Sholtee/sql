/********************************************************************************
*  UnwrappedView.cs                                                             *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;

    internal class UnwrappedView: ViewFactory
    {
        public static Type CreateView(Type type) => Cache.GetOrAdd(type, () =>
        {
            return CreateView
            (
                new MemberDefinition
                (
                    $"Unwrapped{type.Name}",
                    type.GetQueryBase()
                ),
                GetMembers()
            );

            IEnumerable<MemberDefinition> GetMembers() 
            {
                foreach (IGrouping<string, ColumnSelection> grp in type.ExtractColumnSelections().GroupBy(sel => sel.ViewProperty.Name))
                {
                    if (grp.Count() == 1)
                    {
                        ColumnSelection sel = grp.Single();

                        yield return new MemberDefinition
                        (
                            grp.Key,
                            sel.ViewProperty.PropertyType,
                            CustomAttributeBuilderFactory.CreateFrom(sel.Reason)
                        );

                        continue;
                    }

                    int i = 0;

                    foreach (ColumnSelection sel in grp)
                    {
                        //
                        // [BelongsTo(typeof(TTable), column: "Column", ...), MapTo("TView.Column")]
                        // public TValue Column_i {get; set;}
                        //

                        yield return new MemberDefinition
                        (
                            $"{grp.Key}_{i++}",
                            sel.ViewProperty.PropertyType,
                            CustomAttributeBuilderFactory.CreateFrom
                            (
                                sel.Reason,

                                //
                                // A "Column" tulajdonsagot meg ha az eredeti nezet nem is tartalmazta most be kell allitsuk
                                // (mivel a tulajdonsag uj nevet kapott)
                                //

                                new KeyValuePair<PropertyInfo, object>
                                (
                                    typeof(ColumnSelectionAttribute).GetProperty(nameof(ColumnSelectionAttribute.Column)) ?? throw new MissingMemberException(sel.Reason.GetType().Name, nameof(ColumnSelectionAttribute.Column)),
                                    grp.Key
                                )
                            ),
                            CustomAttributeBuilderFactory.CreateFrom<MapToAttribute>(new[] { typeof(string) }, new object[] { sel.ViewProperty.FullName() })
                        );
                    }
                }
            }
        }, nameof(UnwrappedView));
    }
}