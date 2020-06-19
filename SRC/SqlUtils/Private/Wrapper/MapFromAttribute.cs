/********************************************************************************
*  MapFromAttribute.cs                                                          *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Internals
{
    /// <summary>
    /// This is an internal class, don't use it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class MapFromAttribute: Attribute // publikusnak kell lennie h a GetCustomAttribute() megtalalja dinamikus tipusokon
    {
        /// <summary>
        /// 
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// 
        /// </summary>
        public MapFromAttribute(string property) => Property = property;
    }
}
