/********************************************************************************
* TypeExtensionsTest.cs                                                         *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using NUnit.Framework;

namespace Solti.Utils.SQL.Tests
{
    using Internals;
    using Interfaces;
    using Interfaces.DataAnnotations;

    [TestFixture]
    public sealed class TypeExtensionsTest
    {
        [OneTimeSetUp]
        public void SetupFixture() => Config.Use(new DiscoveredDataTables(typeof(WrapperTests).Assembly));

        [Test]
        public void MakeInstanceTest()
        {
            object result = typeof(List<>).MakeInstance(typeof(int));

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<List<int>>());
        }

        private class ListItem
        {
            [BelongsTo(typeof(object))]
            public int Foo { get; set; }
            [PrimaryKey]
            [BelongsTo(typeof(object))]
            public string Bar { get; set; }
        }

        [Test]
        public void GetWrappedSelections_ShouldReturnTheWrappedProperties()
        {
            IReadOnlyList<PropertyInfo> ws = typeof(WrappedView1).GetWrappedSelections();

            Assert.That(ws.Count, Is.EqualTo(1));
            Assert.That(ws[0], Is.EqualTo(typeof(WrappedView1).GetProperty(nameof(WrappedView1.ViewList))));

            ws = typeof(Start_Node_View_ValueList).GetWrappedSelections();
            Assert.That(ws.Count, Is.EqualTo(1));
            Assert.That(ws[0], Is.EqualTo(typeof(Start_Node_View_ValueList).GetProperty(nameof(Start_Node_View_ValueList.References))));
        }

        [Test]
        public void GetWrappedSelections_ShouldCache() =>
            Assert.AreSame(typeof(WrappedView1).GetWrappedSelections(), typeof(WrappedView1).GetWrappedSelections());

        [Test]
        public void GetWrappedSelection_ShouldValidateTheList() => 
            Assert.Throws<InvalidOperationException>(() => typeof(WrappedView5_Bad).GetWrappedSelections());

        [Test]
        public void GetColumnSelections_ShouldHandleViewsBasedOnORMTypes()
        {
            //
            // Az Extension1 nezet a Start_Node ORM tipus leszarmazottja.
            //

            IReadOnlyList<ColumnSelection> selections = typeof(Extension1).GetColumnSelections();

            Assert.That(selections.Where(sel => sel.Kind == SelectionKind.Implicit).All(sel => sel.ViewProperty.DeclaringType == typeof(Start_Node) && sel.Reason is BelongsToAttribute));
            Assert.That(selections.Where(sel => sel.Kind == SelectionKind.Explicit).All(sel => sel.ViewProperty.DeclaringType == typeof(Extension1)));        
        }

        [Test]
        public void GetColumnSelections_ShouldSkipIgnoredProperties()
        {
            // Start_Node.Ignored
            Assert.That(typeof(Extension1).GetProperties().Count(prop => prop.GetCustomAttribute<IgnoreAttribute>() != null), Is.EqualTo(1));
            Assert.That(typeof(Extension1).GetColumnSelections().Where(sel => sel.ViewProperty.GetCustomAttribute<IgnoreAttribute>() != null), Is.Empty);
        }

        [Test]
        public void GetColumnSelections_ShouldCache() =>      
            Assert.AreSame(typeof(Extension1).GetColumnSelections(), typeof(Extension1).GetColumnSelections());

        [Test]
        public void GetColumnSelectionsDeep_ShouldExtractColumnsFromNestedTypes()
        {
            IReadOnlyList<ColumnSelection> sel = typeof(WrappedView2).GetColumnSelectionsDeep();

            Assert.That(sel.Select(s => s.ViewProperty).SequenceEqual(typeof(WrappedView2)
                .GetColumnSelections()
                .Concat(typeof(View1).GetColumnSelections())
                .Concat(typeof(View2).GetColumnSelections())
                .Select(s => s.ViewProperty)));
        }

        [Test]
        public void GetColumnSelectionsDeep_ShouldSkipIgnoredProperties()
        {
            // Start_Node.Ignored
            Assert.That(typeof(Extension1).GetProperties().Count(prop => prop.GetCustomAttribute<IgnoreAttribute>() != null), Is.EqualTo(1));
            Assert.That(typeof(Extension1).GetColumnSelectionsDeep().Where(sel => sel.ViewProperty.GetCustomAttribute<IgnoreAttribute>() != null), Is.Empty);
        }

        [Test]
        public void GetColumnSelectionsDeep_ShouldCache() =>
            Assert.AreSame(typeof(WrappedView1).GetColumnSelectionsDeep(), typeof(WrappedView1).GetColumnSelectionsDeep());

        [Test]
        public void GetBaseOrmType_ShouldReturnTheBaseOrmType()
        {
            Assert.That(typeof(Extension1).GetBaseDataTable(), Is.EqualTo(typeof(Start_Node)));
        }

        [Test]
        public void GetBaseOrmType_ShouldHandleTheOrmTypes()
        {
            Assert.That(typeof(Start_Node).GetBaseDataTable(), Is.EqualTo(typeof(Start_Node)));
        }

        [Test]
        public void GetBaseOrmType_ShouldReturnNullIfNoBase() =>
            Assert.IsNull(typeof(object).GetBaseDataTable());

        [Test]
        public void MakeInstance_ShouldCreateANewInstance()
        {
            object lst_1 = typeof(List<>).MakeInstance(typeof(string));

            Assert.That(lst_1, Is.Not.Null);
            Assert.IsInstanceOf<List<string>>(lst_1);

            object lst_2 = typeof(List<string>).MakeInstance();
            Assert.That(lst_2, Is.Not.Null);
            Assert.IsInstanceOf<List<string>>(lst_2);

            Assert.AreNotSame(lst_1, lst_2);
        }

        [Test]
        public void MakeInstance_ShouldNotHandlePrimitiveTypes() =>
            Assert.Throws<MissingMethodException>(() => typeof(int).MakeInstance());
    }
}
