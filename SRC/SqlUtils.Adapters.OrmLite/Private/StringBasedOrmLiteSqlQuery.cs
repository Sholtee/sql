/********************************************************************************
* StringBasedOrmLiteSqlQuery.cs                                                 *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;

using ServiceStack.OrmLite;

namespace Solti.Utils.SQL.Internals
{
    internal class StringBasedOrmLiteSqlQuery : TypedSqlQuery
    {
        private readonly string FSql;
        private readonly IDbConnection FConnection;

        public StringBasedOrmLiteSqlQuery(IDbConnection connection, string sql)
        {
            FConnection = connection;
            FSql = sql;
        }

        protected override List<TView> Run<TView>() => FConnection.Select<TView>(FSql);
    }
}
