/********************************************************************************
*  MapFromAttribute.cs                                                          *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Internals
{
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class MapFromAttribute: Attribute
    {
        public string Property { get; }

        public MapFromAttribute(string property) => Property = property;
    }
}
