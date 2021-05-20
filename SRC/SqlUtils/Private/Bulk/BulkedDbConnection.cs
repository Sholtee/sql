﻿/********************************************************************************
* BulkedDbConnection.cs                                                         *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using Solti.Utils.Proxy;
using Solti.Utils.Proxy.Attributes;
using Solti.Utils.Proxy.Generators;

[assembly: EmbedGeneratedType(typeof(ProxyGenerator<IDbCommand, Solti.Utils.SQL.Internals.BulkedDbConnection.IDbCommandInterceptor>))]

namespace Solti.Utils.SQL.Internals
{
    using Primitives;

    internal sealed class BulkedDbConnection: IBulkedDbConnection
    {
        internal IDbConnection Connection { get; }

        internal StringBuilder Buffer { get; }

        public BulkedDbConnection(IDbConnection connection)
        {
            if (connection is BulkedDbConnection) throw new InvalidOperationException(); // TODO
            Connection = connection;

            Buffer = new StringBuilder();
        }

        public void Dispose()
        {
            Buffer.Clear();
        }

        public IDbTransaction BeginTransaction() => throw new NotSupportedException();

        public IDbTransaction BeginTransaction(IsolationLevel il) => throw new NotSupportedException();

        public void Close()
        {
        }

        public void ChangeDatabase(string databaseName) => throw new NotSupportedException();

        [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "The class is instantiated by the proxy generator")]
        internal class IDbCommandInterceptor : InterfaceInterceptor<IDbCommand>
        {
            private BulkedDbConnection Parent { get; }

            public IDbCommandInterceptor(BulkedDbConnection parent) : base(parent.Connection.CreateCommand()) => 
                Parent = parent;

            private static readonly Regex FCommandTerminated = new(";\\s*$", RegexOptions.Compiled);

            public override object? Invoke(MethodInfo method, object?[] args, MemberInfo extra)
            {
                switch (method.Name)
                {
                    case nameof(Target.ExecuteNonQuery):
                        string command = Config.Instance.SqlFormat(Target!.CommandText, Target
                            .Parameters
                            .Cast<IDbDataParameter>()
                            .ToArray());

                        if (!FCommandTerminated.IsMatch(command)) command += ";";
                        Parent.Buffer.AppendLine(command);

                        return 0;
                    case nameof(Target.ExecuteReader):
                    case nameof(Target.ExecuteScalar):
                        throw new NotSupportedException();
                }

                return base.Invoke(method, args, extra);
            }
        }

        public IDbCommand CreateCommand() => (IDbCommand) ProxyGenerator<IDbCommand, IDbCommandInterceptor>
            .GetGeneratedType()
            .GetConstructor(new[] { typeof(BulkedDbConnection) })
            .ToStaticDelegate()
            .Invoke(new object[] { this });

        public void Open() => throw new NotSupportedException();

        public string ConnectionString
        {
            get => Connection.ConnectionString;
            set => throw new NotSupportedException();
        }

        public int ConnectionTimeout => Connection.ConnectionTimeout;

        public string Database => Connection.Database;

        public ConnectionState State => Connection.State;

        public int Flush()
        {
            if (Buffer.Length == 0) return 0;

            using IDbCommand cmd = Connection.CreateCommand();
            cmd.CommandText = Buffer.ToString();

            try
            {
                return cmd.ExecuteNonQuery();
            }
            finally
            {
                Buffer.Clear();
            }
        }

        public override string ToString() => Buffer.ToString();
    }
}
