/********************************************************************************
*  UnwrappedView{TView}.cs                                                      *
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
    using Properties;

    internal class UnwrappedView<TView>: ViewFactory<UnwrappedView<TView>>
    {
        protected override Type CreateView()
        {
            Type type = typeof(TView);

            if (!type.IsDatabaseEntityOrView())
            {
                var ex = new InvalidOperationException(Resources.NOT_A_VIEW);
                ex.Data[nameof(type)] = type;
            }

            return CreateView
            (
                new MemberDefinition
                (
                    $"Unwrapped{type.Name}",
                    type.GetQueryBase()
                ),
                GetMembers()
            );
        }

        private static IEnumerable<MemberDefinition> GetMembers() 
        {
            foreach (IGrouping<string, ColumnSelection> grp in typeof(TView).ExtractColumnSelections().GroupBy(sel => sel.ViewProperty.Name))
            {
                if (grp.Count() == 1)
                {
                    //
                    // Ha a property csak egyszer szerepel a kicsomagolt nezetben akkor nem kell modositani a nevet
                    //

                    ColumnSelection sel = grp.Single();

                    yield return new MemberDefinition
                    (
                        grp.Key,
                        sel.ViewProperty.PropertyType,
                        CustomAttributeBuilderFactory.CreateFrom(sel.Reason)
                    );
                }

                else
                {
                    //
                    // Kulonben egyedive kell tenni a tulajdonsag nevet, valamint jelezni kell (MapToAttribute) h mely nezet-tulajdonsaghoz tartozik
                    //

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

                            //
                            // Mappolaskor az eredeti tulajdonsagba kerul vissza a tartalom
                            //

                            CustomAttributeBuilderFactory.CreateFrom<MapToAttribute>(new[] { typeof(string) }, new object[] { sel.ViewProperty.FullName() })
                        );
                    }
                }
            }
        }
    }
}