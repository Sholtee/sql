/********************************************************************************
*  ViewFactory{TDescendant}.cs                                                  *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Internals
{
    internal class ViewFactory<TDescendant>: ViewFactory where TDescendant: ViewFactory<TDescendant>, new()
    {
        private static readonly object FLock = new();

        private static Type? FType;

        protected virtual Type CreateView() => throw new NotImplementedException();

        public static Type Type 
        {
            get 
            {
                if (FType is null)
                    lock (FLock)
                        #pragma warning disable CA1508 // Avoid dead conditional code
                        if (FType is null)
                        #pragma warning restore CA1508
                            FType = new TDescendant().CreateView();
                return FType;
            }
        }
    }
}
