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
    /// This is an internal class, don't use it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class MapToAttribute: Attribute, IBuildableAttribute // publikusnak kell lennie h a GetCustomAttribute() megtalalja dinamikus tipusokon
    {
#if false
        //
        // Igazabol ez igy tok szep es jo volna csak h a runtime elhasal FileNotFoundException-el ha az attributumot CustomAttributeBuilder-rel
        // alkalmazzuk es a "reflectedType" dinamikus tipus
        // 

        /// <summary>
        /// 
        /// </summary>
        public PropertyInfo Property { get; }


        /// <summary>
        /// 
        /// </summary>
        [SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "In practice this is an internal class")]
        public MapToAttribute(Type reflectedType, string property) => Property = reflectedType.GetProperty(property) ?? throw new MissingMemberException(reflectedType.Name, property);
#else
        /// <summary>
        /// 
        /// </summary>
        public string Property { get; }

        /// <summary>
        /// 
        /// </summary>
        public MapToAttribute(string property) => Property = property;
#endif
        CustomAttributeBuilder IBuildableAttribute.GetBuilder(params KeyValuePair<PropertyInfo, object>[] customParameters) => CustomAttributeBuilderFactory.CreateFrom<MapToAttribute>(new[] { typeof(string) }, new object[] { Property });
    }
}
