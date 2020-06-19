/********************************************************************************
*  Unwrapped.cs                                                                 *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Internals
{
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
                            FType = ViewFactory.CreateView
                            (
                                new MemberDefinition
                                (
                                    $"{$"Unwrapped{typeof(TView).Name}"}_Key",
                                    typeof(TView).GetQueryBase()
                                ),
                                typeof(TView).ExtractColumnSelections()
                            );
                return FType;
            }
        }
    }
}