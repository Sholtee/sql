/********************************************************************************
*  PrimaryKeyAttribute.cs                                                       *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Interfaces.DataAnnotations
{
    /// <summary>
    /// Marks a property as a primary key.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PrimaryKeyAttribute : Attribute
    {
    }
}