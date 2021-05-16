/********************************************************************************
*  ClassFactory.cs                                                              *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

using static System.Reflection.Emit.OpCodes;

namespace Solti.Utils.SQL.Internals
{
    /****************************************************************
     * public sealed class MyClass
     * {
     *     public TType_1 Property_1 {get; set;}
     *     
     *     public TType_2 Property_2 {get; set;}
     *     
     *     public override int GetHashCode()
     *     {
     *         HashCode hc = new HashCode();
     *         
     *         hc.Add<TType_1>(this.Property_1);
     *         hc.Add<TType_2>(this.Property_2);
     *         
     *         return hc.ToHashCode();
     *     }
     *     
     *     public override bool Equals(object obj)
     *     {
     *         MyClass that = obj as MyClass;
     *         if (that == null)
     *             return false;
     *             
     *         if (!Object.Equals(this.Property_1, that.Property_1))
     *             return false;
     *         if (!Object.Equals(this.Property_2, that.Property_2))
     *             return false;
     *             
     *         return true; 
     *     }
     * } 
     *****************************************************************/

    internal class ClassFactory
    {
        #region Private
        private const MethodAttributes PUBLIC_OVERRIDE = MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig;

        private interface IMethodGenerator
        {
            void ProcessProperty(PropertyBuilder prop);
            void GenerateEpilogue();
        }

        private sealed class EqualsGenerator : IMethodGenerator 
        {
            private readonly Label FReturnFalse;

            private readonly LocalBuilder FThat;

            private readonly ILGenerator FGenerator;

            //
            // public override bool Equals(object val)
            // {
            //     MyClass that = val as MyClass;
            //     if (that == null)
            //        return false;
            //     
            //     ...
            // }
            //

            public EqualsGenerator(TypeBuilder cls)
            {
                FGenerator = cls
                    .DefineMethod(nameof(Equals), PUBLIC_OVERRIDE, typeof(bool), new[] { typeof(object) })
                    .GetILGenerator();
                FReturnFalse = FGenerator.DefineLabel();
                FThat = FGenerator.DeclareLocal(cls);

                FGenerator.Emit(Ldarg_1);
                FGenerator.Emit(Isinst, cls);
                FGenerator.Emit(Stloc, FThat);
                FGenerator.Emit(Ldloc, FThat);
                FGenerator.Emit(Brfalse, FReturnFalse);
            }

            //
            // if (!Object.Equals(this.Prop, that.Prop))
            //   return false;
            //

            public void ProcessProperty(PropertyBuilder prop)
            {

                FGenerator.Emit(Ldarg_0);
                FGenerator.Emit(Call, prop.GetMethod);
                if (prop.PropertyType.IsValueType)
                    FGenerator.Emit(Box, prop.PropertyType);

                FGenerator.Emit(Ldloc, FThat);
                FGenerator.Emit(Call, prop.GetMethod);
                if (prop.PropertyType.IsValueType)
                    FGenerator.Emit(Box, prop.PropertyType);

                FGenerator.Emit(Call, ((Func<object?, object?, bool>) Object.Equals).Method);
                FGenerator.Emit(Brfalse, FReturnFalse);
            }

            public void GenerateEpilogue()
            {
                FGenerator.Emit(Ldc_I4, 1);
                FGenerator.Emit(Ret);
                FGenerator.MarkLabel(FReturnFalse);
                FGenerator.Emit(Ldc_I4, 0);
                FGenerator.Emit(Ret);
            }
        }

        private sealed class GetHashCodeGenerator : IMethodGenerator
        {
            private readonly LocalBuilder FHk;

            private readonly ILGenerator FGenerator;

            //
            // public override int GetHashCode() { var hc = new HashCode(); ... }
            //

            public GetHashCodeGenerator(TypeBuilder cls)
            {
                FGenerator = cls
                    .DefineMethod(nameof(GetHashCode), PUBLIC_OVERRIDE, typeof(int), Type.EmptyTypes)
                    .GetILGenerator();
                FHk = FGenerator.DeclareLocal(typeof(HashCode));

                FGenerator.Emit(Ldloca, FHk);
                FGenerator.Emit(Initobj, typeof(HashCode));
            }

            //
            // hc.Add<T>(this.Prop);
            //

            private static readonly MethodInfo HkAdd = ((MethodCallExpression) ((Expression<Action<HashCode>>) (hc => hc.Add(0))).Body)
                .Method
                .GetGenericMethodDefinition();

            public void ProcessProperty(PropertyBuilder prop)
            {
                FGenerator.Emit(Ldloca, FHk);
                FGenerator.Emit(Ldarg_0);
                FGenerator.Emit(Call, prop.GetMethod);
                FGenerator.Emit(Call, HkAdd.MakeGenericMethod(prop.PropertyType));
            }

            private static readonly MethodInfo HkToHashCode = ((MethodCallExpression) ((Expression<Action<HashCode>>) (hc => hc.ToHashCode())).Body).Method;

            public void GenerateEpilogue()
            {
                FGenerator.Emit(Ldloca, FHk);
                FGenerator.Emit(Call, HkToHashCode);
                FGenerator.Emit(Ret);
            }
        }

        private readonly TypeBuilder FClass;

        private readonly IMethodGenerator
            FEqualsGenerator,
            FGetHashCodeGenerator;
        #endregion

        #region Public
        public void AddProperty(string name, Type type, params CustomAttributeBuilder[] customAttributes)
        {
            //
            // public XXX {}
            //

            PropertyBuilder property = FClass.DefineProperty(name, PropertyAttributes.None, type, Array.Empty<Type>());

            //
            // FXxX
            //

            FieldBuilder field = FClass.DefineField($"F{name}", type, FieldAttributes.Private);

            //
            // {get => FXxX;}
            //

            MethodBuilder getPropMthdBldr = FClass.DefineMethod(
                $"Get{name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                type,
                Type.EmptyTypes);

            ILGenerator ilGenerator = getPropMthdBldr.GetILGenerator();

            ilGenerator.Emit(Ldarg_0);
            ilGenerator.Emit(Ldfld, field);
            ilGenerator.Emit(Ret);

            //
            // {set => FXxX = value;}
            //

            MethodBuilder setPropMthdBldr = FClass.DefineMethod(
                $"Set{name}",
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
                null,
                new[] { type });

            ilGenerator = setPropMthdBldr.GetILGenerator();

            ilGenerator.Emit(Ldarg_0);
            ilGenerator.Emit(Ldarg_1);
            ilGenerator.Emit(Stfld, field);
            ilGenerator.Emit(Ret);

            //
            // public XXX {get; set; }
            //

            property.SetGetMethod(getPropMthdBldr);
            property.SetSetMethod(setPropMthdBldr);

            FGetHashCodeGenerator.ProcessProperty(property);
            FEqualsGenerator.ProcessProperty(property);

            foreach (CustomAttributeBuilder customAttribute in customAttributes) 
            {
                property.SetCustomAttribute(customAttribute);
            }
        }

        public ClassFactory(string name, params CustomAttributeBuilder[] customAttributes)
        {
            string module = $"{name}_Module";

            FClass = AssemblyBuilder
                .DefineDynamicAssembly(new AssemblyName($"{module}_ASM"), AssemblyBuilderAccess.Run)
                .DefineDynamicModule(module)
                .DefineType(name, TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.Class);

            foreach (CustomAttributeBuilder customAttribute in customAttributes)
            {
                FClass.SetCustomAttribute(customAttribute);
            }

            FEqualsGenerator = new EqualsGenerator(FClass);
            FGetHashCodeGenerator = new GetHashCodeGenerator(FClass);
        }

        public Type CreateType()
        {
            if (FClass.IsCreated())
                throw new InvalidOperationException(); // TODO: message

            FEqualsGenerator.GenerateEpilogue();
            FGetHashCodeGenerator.GenerateEpilogue();

            return FClass.CreateTypeInfo()!.AsType();
        }
        #endregion
    }
}