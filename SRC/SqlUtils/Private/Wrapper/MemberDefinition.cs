/********************************************************************************
*  MemberDefinition.cs                                                          *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    internal sealed class MemberDefinition
    {
        public string Name { get; }
        public Type Type { get; }
        public IReadOnlyList<CustomAttributeBuilder> CustomAttributes { get; }

        public MemberDefinition(string name, Type type, params CustomAttributeBuilder[] customAttributes) 
        {
            Name = name;
            Type = type;
            CustomAttributes = customAttributes;
        }
    }
}
