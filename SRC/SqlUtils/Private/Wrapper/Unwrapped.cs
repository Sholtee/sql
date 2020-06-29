/********************************************************************************
*  Unwrapped.cs                                                                 *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Diagnostics;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
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

                            FType = UnwrappedView.Create(type);

                            Debug.WriteLine($"Unwrapping view: {typeof(TView)}. The result is:{Environment.NewLine}{string.Join<PropertyInfo>(Environment.NewLine, FType.GetProperties())}");
                        }
                    }
                }
                return FType;
            }
        }
    }
}