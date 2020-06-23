/********************************************************************************
*  Unwrapped.cs                                                                 *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Properties;

    internal static class Unwrapped<TView>
    {
        private static readonly object FLock = new object();

        private static Type? FType;

        public static Type Type 
        {
            get 
            {
                if (FType == null)
                {
                    lock (FLock)
                    {
                        if (FType == null)
                        {
                            Type type = typeof(TView);

                            if (!type.IsDatabaseEntityOrView())
                            {
                                var ex = new InvalidOperationException(Resources.NOT_A_VIEW);
                                ex.Data[nameof(type)] = type;
                            }

                            FType = ViewFactory.CreateView
                            (
                                new MemberDefinition
                                (
                                    $"{$"Unwrapped{type.Name}"}",
                                    type.GetQueryBase()
                                ),
                                GetMembers()
                            );

                            Debug.WriteLine($"Unwrapping view: {typeof(TView)}. The result is:{Environment.NewLine}{string.Join<PropertyInfo>(Environment.NewLine, FType.GetProperties())}");
                        }
                    }
                }
                return FType;
            }
        }

        private static IEnumerable<MemberDefinition> GetMembers() 
        {
            foreach (IGrouping<string, ColumnSelection> grp in typeof(TView).ExtractColumnSelections().GroupBy(sel => sel.ViewProperty.Name))
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
    }
}