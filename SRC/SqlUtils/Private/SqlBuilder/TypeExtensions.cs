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
        public static PropertyInfo GetPrimaryKey(this Type viewOrDatabaseEntity) => Cache.GetOrAdd(viewOrDatabaseEntity, () =>
        {
            PropertyInfo? pk;

            //
            // Ha a parameter adattabla v olyan nezet ami adattablabol szarmazik akkor 
            // egyszeruen vissza tudjuk adni az elsodleges kulcsot.
            //

            Type? databaseEntity = viewOrDatabaseEntity.GetBaseDataType();

            if (databaseEntity != null) pk = databaseEntity
                .GetProperties(BINDING_FLAGS)
                .SingleOrDefault(Config.Instance.IsPrimaryKey);

            //
            // Kulomben megkeressuk azt a nezet tulajdonsagot ami az elsodleges kulcsra hivatkozik
            //

            else
            {
                pk = viewOrDatabaseEntity
                    .GetQueryBase()
                    .GetPrimaryKey();

                pk = viewOrDatabaseEntity
                    .GetColumnSelections()
                    .Where(sel => (sel.Reason.Column ?? sel.Column.Name) == pk.Name)
                    .Select(sel => sel.Column)
                    .SingleOrDefault();
            }

            if (pk == null)
            {
                var ex = new MissingMemberException(Resources.NO_PRIMARY_KEY);
                ex.Data[nameof(viewOrDatabaseEntity)] = viewOrDatabaseEntity;
                throw ex;
            }

            return pk;
        });

        public static Type GetQueryBase(this Type viewOrDatabaseEntity) => viewOrDatabaseEntity.GetCustomAttribute<ViewAttribute>(inherit: false)?.Base ?? viewOrDatabaseEntity.GetBaseDataType() ?? throw new InvalidOperationException(Resources.BASE_CANNOT_BE_DETERMINED);
    }
}