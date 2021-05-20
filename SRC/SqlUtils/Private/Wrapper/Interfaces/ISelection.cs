/********************************************************************************
* ISelection.cs                                                                 *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System.Reflection;


namespace Solti.Utils.SQL.Internals
{
    internal interface ISelection
    {
        PropertyInfo ViewProperty { get; }
    }
}
