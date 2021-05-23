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
    /// When a view is being mapped to a <see cref="ValueType"/> this attribute denotes the source property. 
    /// </summary>
    /// <remarks>This is an internal class, you should not use it.</remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class MapFromAttribute: Attribute, IBuildableAttribute, IPropertySelector // publikusnak kell lennie h a GetCustomAttribute() megtalalja dinamikus tipusokon
    {
        /// <summary>
        /// The name of the source property that will provide the <see cref="ValueType"/>.
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// Creates a new <see cref="MapFromAttribute"/> instance.
        /// </summary>
        public MapFromAttribute(string property) => Property = property;

        CustomAttributeBuilder IBuildableAttribute.GetBuilder(params KeyValuePair<PropertyInfo, object>[] customParameters) => CustomAttributeBuilderFactory.CreateFrom<MapFromAttribute>(new[] { typeof(string) }, new object[] { Property });

        /// <inheritdoc/>
        public override string ToString() => $"{nameof(MapFromAttribute)}(\"{Property}\")";
    }
}
