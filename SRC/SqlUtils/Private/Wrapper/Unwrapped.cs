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
        private static readonly object FLock = new object();

        private static Type? FType;

        public static Type Type 
        {
            get 
            {
                if (FType == null)
                    lock (FLock)
                        if (FType == null)
                            FType = GetUnwrappedType();
                return FType;
            }
        }

        private static Type GetUnwrappedType()
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

            return tb.CreateTypeInfo()!.AsType();
        }
    }
}