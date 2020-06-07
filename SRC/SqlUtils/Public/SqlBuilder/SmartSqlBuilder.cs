/********************************************************************************
*  SmartSqlBuilder.cs                                                           *
*                                                                               *
*  Author: Denes Solti                                                          *
********************************************************************************/
using System;
using System.Diagnostics.CodeAnalysis;

namespace Solti.Utils.SQL
{
    using Interfaces;
    using Internals;
    using Properties;

    /// <summary>
    /// Builds the SQL query from the given view. 
    /// </summary>
    public static class SmartSqlBuilder<TView, TBasePoco>
    {
        private static Action<ISqlQuery>? FBuild;

        /// <summary>
        /// Represents the build action related to the given <typeparamref name="TView"/> and <typeparamref name="TBasePoco"/>.
        /// </summary>
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "It's intended because all the specialized builders must have their own build method.")]
        public static Action<ISqlQuery> Build
        {
            get
            {
                if (FBuild == null)
                    throw new InvalidOperationException(Resources.UNINITIALIZED);

                return FBuild;
            }
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
                new JoinActionGenerator<TView, TBasePoco>(customEdges),
                new FragmentActionGenerator<TView>(),
                new FinalActionGenerator<TView>()
            );
        }
    }
}