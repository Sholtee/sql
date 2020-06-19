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
        public PropertyInfo ViewProperty { get; }
    }
}
