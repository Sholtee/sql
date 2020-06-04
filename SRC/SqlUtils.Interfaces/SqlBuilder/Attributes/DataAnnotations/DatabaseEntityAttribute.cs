/********************************************************************************
*  DatabaseEntityAttribute.cs                                                   *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Interfaces.DataAnnotations
{
    /// <summary>
    /// ORM entity marker.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class DatabaseEntityAttribute : Attribute
    {
    }
}