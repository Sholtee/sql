/********************************************************************************
* Init.cs                                                                       *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System.Runtime.CompilerServices;

using ServiceStack;

namespace Solti.Utils.SQL.OrmLite.Tests
{
    internal static class Init
    {
        [ModuleInitializer]
        public static void RegisterOSSLicense() =>
            /*
            //
            // Licensz innen veve: https://docs.servicestack.net/oss#oss-license-key
            //

            Licensing.RegisterLicense("1001-e1JlZjoxMDAxLE5hbWU6VGVzdCBCdXNpbmVzcyxUeXBlOkJ1c2luZXNzLEhhc2g6UHVNTVRPclhvT2ZIbjQ5MG5LZE1mUTd5RUMzQnBucTFEbTE3TDczVEF4QUNMT1FhNXJMOWkzVjFGL2ZkVTE3Q2pDNENqTkQyUktRWmhvUVBhYTBiekJGUUZ3ZE5aZHFDYm9hL3lydGlwUHI5K1JsaTBYbzNsUC85cjVJNHE5QVhldDN6QkE4aTlvdldrdTgyTk1relY2eis2dFFqTThYN2lmc0JveHgycFdjPSxFeHBpcnk6MjAxMy0wMS0wMX0=");
            */

            //
            // https://account.servicestack.net/trial
            //

            Licensing.RegisterLicense("TRIAL30WEB-e1JlZjpUUklBTDMwV0VCLE5hbWU6NS8yMy8yMDIxIDNhMTU5NDVjYmNiMTRmZGI5NTI0MjY5YWQ4OWM4YzUzLFR5cGU6VHJpYWwsTWV0YTowLEhhc2g6cDZ6VXVZdEF3U0hjMzFpczlubCs5RFdjRVZzN1RRTCt4Q0t3SkQrQ3JyM2JlUU1TZjE4d1BwSXFJc2tGSTZ6cE96VmtNdWp5Uy9mckxKZTVFU2RYV2ZxNXhYaHRuRlFlSk5vZ1NuQW9raE1weDI2M1JPaGxZYUhUZzNLR0crZTMwV1RzVU1lMHFkdlF1YlZjSm5WVndaZUd3MXpmcEtWZXFTRnJjeXNyb2VFPSxFeHBpcnk6MjAyMS0wNi0yMn0=");
    }
}
