/********************************************************************************
* Config.cs                                                                     *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Solti.Utils.SQL
{
    /// <summary>
    /// Exposes the configuration of this library.
    /// </summary>
    public static class Config
    {
        /// <summary>
        /// The config instance.
        /// </summary>
        public static IConfig Instance { get; private set; }

        /// <summary>
        /// Uses the given config.
        /// </summary>
        public static void Use<T>() where T : IConfig, new() => Use(new T());

        /// <summary>
        /// Uses the given config.
        /// </summary>
        public static void Use<T>(T instance) where T : IConfig => Instance = instance ?? throw new ArgumentNullException(nameof(instance));

        //[SuppressMessage("", "CS8618:Non-nullable property is uninitialized.", Justification = "Instance is never null")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        static Config()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {
            Use<DefaultConfig>();
        }
    }
}
