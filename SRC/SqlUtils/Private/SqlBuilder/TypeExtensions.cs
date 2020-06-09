/********************************************************************************
* TypeExtensions.cs                                                             *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Linq;
using System.Reflection;

namespace Solti.Utils.SQL.Internals
{
    using Interfaces;
    using Primitives;
    using Properties;

    internal static partial class TypeExtensions
    {
        public static PropertyInfo GetPrimaryKey(this Type type) => Cache.GetOrAdd(type, () =>
        {
            PropertyInfo? pk = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .SingleOrDefault(Config.Instance.IsPrimaryKey);

            if (pk == null)
            {
                var ex = new MissingMemberException(Resources.NO_PRIMARY_KEY);
                ex.Data[nameof(type)] = type;
                throw ex;
            }

            return pk;
        });

        public static Type GetQueryBase(this Type type) => type.GetCustomAttribute<ViewAttribute>(inherit: false)?.Base ?? type.GetBaseDataType() ?? throw new InvalidOperationException(Resources.BASE_CANNOT_BE_DETERMINED);
    }
}