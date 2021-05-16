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
        private readonly Label FReturnFalse;

        private readonly LocalBuilder FThat;

        private readonly ILGenerator FEqualsGenerator;

        private readonly LocalBuilder FHk;

        private readonly ILGenerator  FGetHashCodeGenerator;

        private readonly TypeBuilder FClass; 

        private void GenerateEqualityComparison(PropertyInfo prop) 
        {
            //
            // if (!Object.Equals(this.Prop, that.Prop))
            //   return false;
            //

            FEqualsGenerator.Emit(Ldarg_0);
            FEqualsGenerator.Emit(Call, prop.GetMethod);
            if (prop.PropertyType.IsValueType)
                FEqualsGenerator.Emit(Box, prop.PropertyType);

            FEqualsGenerator.Emit(Ldloc, FThat);
            FEqualsGenerator.Emit(Call, prop.GetMethod);
            if (prop.PropertyType.IsValueType)
                FEqualsGenerator.Emit(Box, prop.PropertyType);

            FEqualsGenerator.Emit(Call, ((Func<object?, object?, bool>) Object.Equals).Method);
            FEqualsGenerator.Emit(Brfalse, FReturnFalse);
        }

        private void GenerateEqualsEpilogue() 
        {
            FEqualsGenerator.Emit(Ldc_I4, 1);
            FEqualsGenerator.Emit(Ret);
            FEqualsGenerator.MarkLabel(FReturnFalse);
            FEqualsGenerator.Emit(Ldc_I4, 0);
            FEqualsGenerator.Emit(Ret);
        }

        private static readonly MethodInfo HkAdd = ((MethodCallExpression) ((Expression<Action<HashCode>>) (hc => hc.Add(0))).Body)
            .Method
            .GetGenericMethodDefinition();

        private void GenerateHashCodeAddition(PropertyInfo prop) 
        {
            //
            // hc.Add<T>(this.Prop);
            //

            FGetHashCodeGenerator.Emit(Ldloca, FHk);
            FGetHashCodeGenerator.Emit(Ldarg_0);
            FGetHashCodeGenerator.Emit(Call, prop.GetMethod);
            FGetHashCodeGenerator.Emit(Call, HkAdd.MakeGenericMethod(prop.PropertyType));
        }

        private static readonly MethodInfo HkToHashCode = ((MethodCallExpression) ((Expression<Action<HashCode>>) (hc => hc.ToHashCode())).Body).Method;

        private void GenerateGetHashCodeEpilogue() 
        {
            FGetHashCodeGenerator.Emit(Ldloca, FHk);
            FGetHashCodeGenerator.Emit(Call, HkToHashCode);
            FGetHashCodeGenerator.Emit(Ret);
        }
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

            GenerateEqualityComparison(property);
            GenerateHashCodeAddition(property);

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

            const MethodAttributes PUBLIC_OVERRIDE = MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig;

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

            FEqualsGenerator = FClass
                .DefineMethod(nameof(Equals), PUBLIC_OVERRIDE, typeof(bool), new[] { typeof(object) })
                .GetILGenerator();
            FReturnFalse = FEqualsGenerator.DefineLabel();
            FThat = FEqualsGenerator.DeclareLocal(FClass);
            FEqualsGenerator.Emit(Ldarg_1);
            FEqualsGenerator.Emit(Isinst, FClass);
            FEqualsGenerator.Emit(Stloc, FThat);
            FEqualsGenerator.Emit(Ldloc, FThat);
            FEqualsGenerator.Emit(Brfalse, FReturnFalse);

            //
            // public override int GetHashCode() { var hc = new HashCode(); ... }
            //

            FGetHashCodeGenerator = FClass
                .DefineMethod(nameof(GetHashCode), PUBLIC_OVERRIDE, typeof(int), Type.EmptyTypes)
                .GetILGenerator();
            FHk = FGetHashCodeGenerator.DeclareLocal(typeof(HashCode));
            FGetHashCodeGenerator.Emit(Ldloca, FHk);
            FGetHashCodeGenerator.Emit(Initobj, typeof(HashCode));
        }

        public Type CreateType()
        {
            if (FClass.IsCreated())
                throw new InvalidOperationException(); // TODO: message

            GenerateEqualsEpilogue();
            GenerateGetHashCodeEpilogue();

            return FClass.CreateTypeInfo()!.AsType();
        }
        #endregion
    }
}