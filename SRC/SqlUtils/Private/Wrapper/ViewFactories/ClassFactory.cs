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
    internal class ClassFactory
    {
        #region Private
        private Label ReturnFalse { get; }

        private ILGenerator EqualsGenerator { get; }

        private LocalBuilder Hk { get; }

        private ILGenerator GetHashCodeGenerator { get; }

        private void GenerateEqualityComparison(PropertyInfo prop) 
        {
            //
            // if (!Object.Equals(this.Prop, ((MyClass) b).Prop))
            //   return false;
            //

            EqualsGenerator.Emit(Ldarg_0);
            EqualsGenerator.Emit(Call, prop.GetMethod);
            if (prop.PropertyType.IsValueType)
                EqualsGenerator.Emit(Box, prop.PropertyType);

            EqualsGenerator.Emit(Ldarg_1);
            EqualsGenerator.Emit(Castclass, Class);
            EqualsGenerator.Emit(Call, prop.GetMethod);
            if (prop.PropertyType.IsValueType)
                EqualsGenerator.Emit(Box, prop.PropertyType);

            EqualsGenerator.Emit(Call, ((Func<object?, object?, bool>) Object.Equals).Method);
            EqualsGenerator.Emit(Brfalse, ReturnFalse);
        }

        private void GenerateEqualsEpilogue() 
        {
            EqualsGenerator.Emit(Ldc_I4, 1);
            EqualsGenerator.Emit(Ret);
            EqualsGenerator.MarkLabel(ReturnFalse);
            EqualsGenerator.Emit(Ldc_I4, 0);
            EqualsGenerator.Emit(Ret);
        }

        private static readonly MethodInfo HkAdd = ((MethodCallExpression) ((Expression<Action<HashCode>>) (hc => hc.Add(0))).Body)
            .Method
            .GetGenericMethodDefinition();

        private void GenerateHashCodeAddition(PropertyInfo prop) 
        {
            //
            // hc.Add<T>(this.Prop);
            //

            GetHashCodeGenerator.Emit(Ldloca, Hk);
            GetHashCodeGenerator.Emit(Ldarg_0);
            GetHashCodeGenerator.Emit(Call, prop.GetMethod);
            GetHashCodeGenerator.Emit(Call, HkAdd.MakeGenericMethod(prop.PropertyType));
        }

        private static readonly MethodInfo HkToHashCode = ((MethodCallExpression) ((Expression<Action<HashCode>>) (hc => hc.ToHashCode())).Body).Method;

        private void GenerateGetHashCodeEpilogue() 
        {
            GetHashCodeGenerator.Emit(Ldloca, Hk);
            GetHashCodeGenerator.Emit(Call, HkToHashCode);
            GetHashCodeGenerator.Emit(Ret);
        }
        #endregion

        #region Protected
        protected PropertyBuilder AddProperty(string name, Type type)
        {
            //
            // public XXX {}
            //

            PropertyBuilder property = Class.DefineProperty(name, PropertyAttributes.None, type, Array.Empty<Type>());

            //
            // FXxX
            //

            FieldBuilder field = Class.DefineField($"F{name}", type, FieldAttributes.Private);

            //
            // {get => FXxX;}
            //

            MethodBuilder getPropMthdBldr = Class.DefineMethod(
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

            MethodBuilder setPropMthdBldr = Class.DefineMethod(
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

            return property;
        }

        protected TypeBuilder Class { get; }
        #endregion

        #region Public
        public ClassFactory(string name)
        {
            Class = AssemblyBuilder
                .DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run)
                .DefineDynamicModule("MainModule")
                .DefineType(name, TypeAttributes.Public | TypeAttributes.Class);

            const MethodAttributes PUBLIC_OVERRIDE = MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig;

            //
            // public override bool Equals(object val) { ... }
            //

            EqualsGenerator = Class
                .DefineMethod(nameof(Equals), PUBLIC_OVERRIDE, typeof(bool), new[] { typeof(object) })
                .GetILGenerator();
            ReturnFalse = EqualsGenerator.DefineLabel();

            //
            // public override int GetHashCode() { var hc = new HashCode(); ... }
            //

            GetHashCodeGenerator = Class
                .DefineMethod(nameof(GetHashCode), PUBLIC_OVERRIDE, typeof(int), Type.EmptyTypes)
                .GetILGenerator();
            Hk = GetHashCodeGenerator.DeclareLocal(typeof(HashCode));
            GetHashCodeGenerator.Emit(Ldloca, Hk);
            GetHashCodeGenerator.Emit(Initobj, typeof(HashCode));
        }

        public Type CreateType()
        {
            if (Class.IsCreated())
                throw new InvalidOperationException(); // TODO: message

            GenerateEqualsEpilogue();
            GenerateGetHashCodeEpilogue();

            return Class.CreateTypeInfo()!.AsType();
        }
        #endregion
    }
}