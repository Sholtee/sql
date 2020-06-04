/********************************************************************************
* TestsBase.cs                                                                  *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    using Interfaces;

    public abstract class TestsBase
    {
        private IConfig FPrevious;

        [OneTimeSetUp]
        public void Setup()
        {
            FPrevious = Config.Instance;
            Config.Use<DefaultConfig>();
        }

        [OneTimeTearDown]
        public void Teardown() => Config.Use(FPrevious);
    }
}
