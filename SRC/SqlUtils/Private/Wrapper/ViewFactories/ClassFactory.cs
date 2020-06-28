/********************************************************************************
*  ClassFactory.cs                                                              *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Solti.Utils.SQL.Internals
{
    internal class ClassFactory
    {
        protected static PropertyBuilder AddProperty(TypeBuilder tb, string name, Type type)
        {
            //
            // public XXX {}
            //

            PropertyBuilder property = tb.DefineProperty(name, PropertyAttributes.None, type, Array.Empty<Type>());

            //
            // FXxX
            //

            FieldBuilder field = tb.DefineField($"F{name}", type, FieldAttributes.Private);

            //
            // {get => FXxX;}
            //

            MethodBuilder getPropMthdBldr = tb.DefineMethod(
                $"Get{name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                type,
                Type.EmptyTypes);

            ILGenerator ilGenerator = getPropMthdBldr.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, field);
            ilGenerator.Emit(OpCodes.Ret);

            //
            // {set => FXxX = value;}
            //

            MethodBuilder setPropMthdBldr = tb.DefineMethod(
                $"Set{name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new[] { type });

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

        protected static TypeBuilder CreateBuilder(string name) => AssemblyBuilder
            .DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run)
            .DefineDynamicModule("MainModule")
            .DefineType(name, TypeAttributes.Public | TypeAttributes.Class);
    }
}