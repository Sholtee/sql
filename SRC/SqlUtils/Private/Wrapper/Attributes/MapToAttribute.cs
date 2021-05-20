/********************************************************************************
*  MapToAttribute.cs                                                            *
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
    /// Marks a property as a mapping source.
    /// </summary>
    /// <remarks>This is an internal class, you should not use it.</remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class MapToAttribute: Attribute, IBuildableAttribute, IPropertySelector // publikusnak kell lennie h a GetCustomAttribute() megtalalja dinamikus tipusokon
    {
        /// <summary>
        /// The name of the destination property.
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// Creates a new <see cref="MapToAttribute"/> instance.
        /// </summary>
        public MapToAttribute(string property) => Property = property;

        CustomAttributeBuilder IBuildableAttribute.GetBuilder(params KeyValuePair<PropertyInfo, object>[] customParameters) => CustomAttributeBuilderFactory.CreateFrom<MapToAttribute>(new[] { typeof(string) }, new object[] { Property });
    }
}
