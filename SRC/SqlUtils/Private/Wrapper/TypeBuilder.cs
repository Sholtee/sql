/********************************************************************************
*  TypeBuilder.cs                                                               *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    internal static class MyTypeBuilder
    {
        public static PropertyBuilder AddProperty(this TypeBuilder tb, PropertyInfo prop)
        {
            //
            // public XXX {}
            //

            PropertyBuilder property = tb.DefineProperty(prop.Name, prop.Attributes, prop.PropertyType, Array.Empty<Type>());

            //
            // FXxX
            //

            FieldBuilder field = tb.DefineField($"F{prop.Name}", prop.PropertyType, FieldAttributes.Private);

            //
            // {get => FXxX;}
            //

            MethodBuilder getPropMthdBldr = tb.DefineMethod(
                $"Get{prop.Name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                prop.PropertyType,
                Type.EmptyTypes);

            ILGenerator ilGenerator = getPropMthdBldr.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Ret);

            //
            // {set => FXxX = value;}
            //

            MethodBuilder setPropMthdBldr = tb.DefineMethod(
                $"Set{prop.Name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new[] { prop.PropertyType });

            ilGenerator = setPropMthdBldr.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.Emit(OpCodes.Stfld, field);
            ilGenerator.Emit(OpCodes.Ret);

            //
            // public XXX {get; set; }
            //

            property.SetGetMethod(getPropMthdBldr);
            property.SetSetMethod(setPropMthdBldr);

            return property;
        }

        public static TypeBuilder Create(string name) => AssemblyBuilder
            .DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run)
            .DefineDynamicModule("MainModule")
            .DefineType(name, TypeAttributes.Public | TypeAttributes.Class);
    }
}