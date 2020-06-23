/********************************************************************************
*  MapFromAttribute.cs                                                          *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;

    /// <summary>
    /// This is an internal class, don't use it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MapFromAttribute: Attribute, IBuildableAttribute // publikusnak kell lennie h a GetCustomAttribute() megtalalja dinamikus tipusokon
    {
        /// <summary>
        /// 
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// 
        /// </summary>
        public MapFromAttribute(string property) => Property = property;

        CustomAttributeBuilder IBuildableAttribute.GetBuilder(params KeyValuePair<PropertyInfo, object>[] customParameters) => CustomAttributeBuilderFactory.CreateFrom<MapFromAttribute>(new[] { typeof(string) }, new object[] { Property });
    }
}
