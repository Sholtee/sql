/********************************************************************************
* Config.cs                                                                     *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;

namespace Solti.Utils.SQL
{
    using Interfaces;
    using Primitives.Patterns;

    /// <summary>
    /// Exposes the configuration of this library.
    /// </summary>
    public static class Config
    {
#if !DEBUG
        private static readonly WriteOnce<IConfig> FInstance = new();
        private static readonly WriteOnce<IKnownDataTables> FKnownTables = new();

        /// <summary>
        /// The config instance.
        /// </summary>
        public static IConfig Instance { get => FInstance.Value!; private set => FInstance.Value = value; }

        /// <summary>
        /// See <see cref="IKnownDataTables"/>.
        /// </summary>
        public static IKnownDataTables KnownTables { get => FKnownTables.Value!; private set => FKnownTables.Value = value; }
#else
        /// <summary>
        /// The config instance.
        /// </summary>
        public static IConfig Instance { get; private set; }

        /// <summary>
        /// See <see cref="IKnownDataTables"/>.
        /// </summary>
        public static IKnownDataTables KnownTables { get; private set; }

        static Config()
        {
            Instance = new DefaultConfig();
            KnownTables = new SpecifiedDataTables();
        }

        internal static IDisposable UseTemporarily(IConfig instance) 
        {
            Use(instance);
            return new ConfigScope();
        }

        private sealed class ConfigScope : Disposable 
        {
            protected override void Dispose(bool disposeManaged)
            {
                Use<DefaultConfig>();
                base.Dispose(disposeManaged);
            }
        }
#endif
        /// <summary>
        /// Uses the given config.
        /// </summary>
        public static void Use<T>() where T : new() 
        {
            if (typeof(IConfig).IsAssignableFrom(typeof(T))) Use((IConfig) new T());
            if (typeof(IKnownDataTables).IsAssignableFrom(typeof(T))) Use((IKnownDataTables) new T());
        }

        /// <summary>
        /// Uses the given config.
        /// </summary>
        public static void Use(IConfig instance) => Instance = instance ?? throw new ArgumentNullException(nameof(instance));

        /// <summary>
        /// Uses the given ORM types.
        /// </summary>
        public static void Use(IKnownDataTables instance) => KnownTables = instance ?? throw new ArgumentNullException(nameof(instance));
    }
}
