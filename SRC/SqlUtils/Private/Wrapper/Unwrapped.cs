/********************************************************************************
*  Unwrapped.cs                                                                 *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    internal static class Unwrapped<TView>
    {
        public static Type Type { get; }

        static Unwrapped()
        {
            Type viewType = typeof(TView);
            TypeBuilder tb = MyTypeBuilder.Create($"Unwrapped{viewType.Name}");

            //
            // Property-k masolasa.
            //

            foreach (ColumnSelection sel in viewType.ExtractColumnSelections())
            {
                tb
                    .AddProperty(sel.Column)
                    .SetCustomAttribute(((IBuildable) sel.Reason).Builder);
            }

            Type = tb.CreateTypeInfo()!.AsType();
        }
    }
}