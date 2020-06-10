/********************************************************************************
* PropertyInfoExtensions.cs                                                     *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using ServiceStack.OrmLite;

namespace Solti.Utils.SQL.Internals
{
    internal static class PropertyInfoExtensions
    {
        public static ModelDefinition GetModelDefinition(this PropertyInfo prop) => prop.ReflectedType.GetModelMetadata(); // Mar gyorsitotarazott
 
        public static FieldDefinition GetFieldDefinition(this PropertyInfo prop) => prop
            .GetModelDefinition()
            .FieldDefinitions
            .SingleOrDefault(fld => fld.PropertyInfo == prop);

        public static string ToSelectString(this PropertyInfo prop)
        {
            IOrmLiteDialectProvider dialectProvider = OrmLiteConfig.DialectProvider;

            return $"{dialectProvider.GetQuotedTableName(prop.GetModelDefinition())}.{dialectProvider.GetQuotedColumnName(prop.GetFieldDefinition().FieldName)}";
        }

        public static LambdaExpression ToSelectExpression(this PropertyInfo prop)
        {
            ParameterExpression para = Expression.Parameter(prop.ReflectedType);

            return Expression.Lambda
            (
                typeof(Func<,>).MakeGenericType(prop.ReflectedType, typeof(object)),
                Expression.Convert
                (
                    Expression.Property(para, prop),
                    typeof(object)
                ),
                para
            );
        }

        public static LambdaExpression ToEqualsExpression(this PropertyInfo @this, PropertyInfo that)
        {
            ParameterExpression 
                para1 = Expression.Parameter(@this.ReflectedType),
                para2 = Expression.Parameter(@this.ReflectedType);

            return Expression.Lambda
            (
                typeof(Func<,,>).MakeGenericType(@this.ReflectedType, that.ReflectedType, typeof(bool)),
                Expression.Equal
                (
                    Expression.Property(para1, @this),
                    Expression.Property(para2, that)
                ),
                para1,
                para2
            );
        }
    }
}
