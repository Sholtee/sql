/********************************************************************************
* SmartSqlBuilder.cs                                                            *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Diagnostics.CodeAnalysis;

namespace Solti.Utils.SQL
{
    using Interfaces;
    using Internals;
    using Properties;

    /// <summary>
    /// Builds the SQL query for the given view. 
    /// </summary>
    public static class SmartSqlBuilder<TView>
    {
        private static Action<ISqlQuery>? FBuild;

        /// <summary>
        /// Represents the build action related to the given <typeparamref name="TView"/>.
        /// </summary>
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "It's intended because all the specialized builders must have their own build method.")]
        public static TQuery Build<TQuery>(Func<Type, TQuery> factory) where TQuery: ISqlQuery
        {
            if (factory is null)
                throw new ArgumentNullException(nameof(factory));

            if (FBuild is null)
                throw new InvalidOperationException(Resources.UNINITIALIZED_BUILDER);

            TQuery query = factory(typeof(TView).GetQueryBase());

            FBuild.Invoke(query);
            return query;
        }

        /// <summary>
        /// Initializes this builder.
        /// </summary>
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "It's intended because all the specialized builders must have their own setup method.")]
        public static void Initialize(params Edge[] customEdges)
        {
            if (customEdges == null)
                throw new ArgumentNullException(nameof(customEdges));

            FBuild = Compiler.Compile
            (
                new JoinActionGenerator<TView>(customEdges),
                new FragmentActionGenerator<TView>()
            );
        }
    }
}