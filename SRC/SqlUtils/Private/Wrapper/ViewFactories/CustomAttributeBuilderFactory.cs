/********************************************************************************
*  CustomAttributeBuilderFactory.cs                                             *
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

    internal static class CustomAttributeBuilderFactory
    {
        public static CustomAttributeBuilder CreateFrom<TAttribute>(Type[] argTypes, object?[] args) where TAttribute : Attribute => new
        (
            typeof(TAttribute).GetConstructor(argTypes) ?? throw new MissingMethodException(typeof(TAttribute).Name, "Ctor"),
            args
        );

        public static CustomAttributeBuilder CreateFrom(IBuildableAttribute attribute, params KeyValuePair<PropertyInfo, object>[] customParameters) => attribute.GetBuilder(customParameters);
    }
}
