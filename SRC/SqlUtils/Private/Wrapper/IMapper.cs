/********************************************************************************
* IMapper.cs                                                                    *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;

namespace Solti.Utils.SQL.Internals
{
    internal interface IMapper
    {
        void RegisterMapping(Type srcType, Type dstType);

        object? MapTo(Type srcType, Type dstType, object? source);
    }
}