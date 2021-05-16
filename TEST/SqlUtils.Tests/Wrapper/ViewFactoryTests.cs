/********************************************************************************
* ViewFactoryTests.cs                                                           *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;

using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    using Internals;

    [TestFixture]
    public class ViewFactoryTests
    {
        private static readonly Type ViewType = ViewFactory.CreateView
        (
            new MemberDefinition
            (
                "MyView",
                typeof(Object)
            ),
            new[]
            {
                new MemberDefinition
                (
                    "Id",
                    typeof(long)
                ),

                new MemberDefinition
                (
                    "Column",
                    typeof(string)
                )
            }
        );

        private static object CreateObjWithValues(long id, string column)
        {
            object result = Activator.CreateInstance(ViewType);
            ViewType.GetProperty("Id").FastSetValue(result, id);
            ViewType.GetProperty("Column").FastSetValue(result, column);
            return result;
        }

        public static IEnumerable<(object A, object B, bool Equal)> Values 
        {

            get 
            {
                object obj = CreateObjWithValues(10, "cica");

                yield return (obj, obj, true);
                yield return (CreateObjWithValues(10, "cica"), CreateObjWithValues(10, "cica"), true);
                yield return (CreateObjWithValues(20, "cica"), CreateObjWithValues(10, "cica"), false);
                yield return (CreateObjWithValues(10, "cica"), CreateObjWithValues(10, "kutya"), false);
                yield return (CreateObjWithValues(10, "cica"), null, false);
                yield return (CreateObjWithValues(10, "cica"), new { Id = 10, Column = "cica" }, false);
            }
        }

        [TestCaseSource(nameof(Values))]
        public void CreateType_ShouldGenerateRecordLikeType_EqualsTest((object A, object B, bool Equal) data) => Assert.That(data.A.Equals(data.B), Is.EqualTo(data.Equal));

        [Test]
        public void CreateType_ShouldGenerateRecordLikeType_GetHashCodeTest()
        {
            var hc = new HashCode();
            hc.Add(1986);
            hc.Add("cica");

            Assert.AreEqual(CreateObjWithValues(1986, "cica").GetHashCode(), hc.ToHashCode());
        }
    }
}
