/********************************************************************************
*  MapToAttribute.cs                                                            *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    /// <summary>
    /// This is an internal class, don't use it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class MapToAttribute: Attribute // publikusnak kell lennie h a GetCustomAttribute() megtalalja dinamikus tipusokon
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
        public MapToAttribute(string propertyFullName) => Property = propertyFullName;
#endif
    }
}
