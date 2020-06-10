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
    using Properties;

    [TestFixture]
    public sealed class TypeExtensionsTest
    {
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
            [EmptyListMarker]
            [BelongsTo(typeof(object))]
            public string Bar { get; set; }
        }

        [Test]
        public void GetWrappedSelections_ShouldReturnTheDesiredWSObject()
        {
            IReadOnlyList<WrappedSelection> ws = typeof(WrappedView1).GetWrappedSelections();

            Assert.That(ws, Is.Not.Null);
            Assert.That(ws.Count, Is.EqualTo(1));
            Assert.That(ws[0].UnderlyingType, Is.EqualTo(typeof(View3)));
            Assert.That(ws[0].Info.GetCustomAttribute<WrappedAttribute>(), Is.Not.Null);         
        }

        [Test]
        public void GetWrappedSelections_ShouldCache() =>
            Assert.AreSame(typeof(WrappedView1).GetWrappedSelections(), typeof(WrappedView1).GetWrappedSelections());

        [Test]
        public void GetWrappedSelection_ShouldValidateTheList() => 
            Assert.Throws<ArgumentException>(() => typeof(WrappedView5_Bad).GetWrappedSelections());

        [Test]
        public void GetColumnSelections_ShouldHandleViewsBasedOnORMTypes()
        {
            //
            // Az Extension1 nezet a Start_Node ORM tipus leszarmazottja.
            //

            IReadOnlyList<ColumnSelection> selections = typeof(Extension1).GetColumnSelections();

            Assert.That(selections.Where(sel => sel.Kind == SelectionKind.Implicit).All(sel => sel.Column.DeclaringType == typeof(Start_Node) && sel.Reason is BelongsToAttribute));
            Assert.That(selections.Where(sel => sel.Kind == SelectionKind.Explicit).All(sel => sel.Column.DeclaringType == typeof(Extension1)));        
        }

        [Test]
        public void GetColumnSelections_ShouldSkipIgnoredProperties()
        {
            // Start_Node.Ignored
            Assert.That(typeof(Extension1).GetProperties().Count(prop => prop.GetCustomAttribute<IgnoreAttribute>() != null), Is.EqualTo(1));
            Assert.That(typeof(Extension1).GetColumnSelections().Where(sel => sel.Column.GetCustomAttribute<IgnoreAttribute>() != null), Is.Empty);
        }

        [Test]
        public void GetColumnSelections_ShouldCache() =>      
            Assert.AreSame(typeof(Extension1).GetColumnSelections(), typeof(Extension1).GetColumnSelections());

        [Test]
        public void ExtractColumnSelections_ShouldExtractColumnsFromNestedTypes()
        {
            IReadOnlyList<ColumnSelection> sel = typeof(WrappedView2).ExtractColumnSelections();

            Assert.That(sel.Select(s => s.Column).SequenceEqual(typeof(WrappedView2)
                .GetColumnSelections()
                .Concat(typeof(View1).GetColumnSelections())
                .Concat(typeof(View2).GetColumnSelections())
                .Select(s => s.Column)));
        }

        [Test]
        public void ExtractColumnSelections_ShouldSkipIgnoredProperties()
        {
            // Start_Node.Ignored
            Assert.That(typeof(Extension1).GetProperties().Count(prop => prop.GetCustomAttribute<IgnoreAttribute>() != null), Is.EqualTo(1));
            Assert.That(typeof(Extension1).ExtractColumnSelections().Where(sel => sel.Column.GetCustomAttribute<IgnoreAttribute>() != null), Is.Empty);
        }

        [Test]
        public void ExtractColumnSelections_ShouldThrowOnPropertyCollision() =>
            Assert.Throws<InvalidOperationException>(() => typeof(WrappedView1_Bad).ExtractColumnSelections(), Resources.PROPERTY_NAME_COLLISSION);

        [Test]
        public void ExtractColumnSelections_ShouldCache() =>
            Assert.AreSame(typeof(WrappedView1).ExtractColumnSelections(), typeof(WrappedView1).ExtractColumnSelections());

        [Test]
        public void GetBaseOrmType_ShouldReturnTheBaseOrmType() =>
            Assert.That(typeof(Extension1).GetBaseDataType(), Is.EqualTo(typeof(Start_Node)));

        [Test]
        public void GetBaseOrmType_ShouldHandleTheOrmTypes() =>
            Assert.That(typeof(Start_Node).GetBaseDataType(), Is.EqualTo(typeof(Start_Node)));

        [Test]
        public void GetBaseOrmType_ShouldReturnNullIfNoBase() =>
            Assert.IsNull(typeof(object).GetBaseDataType());

        [Test]
        public void GetDefaultValue_ShouldHandlePrimitiveTypes() =>
            Assert.That(typeof(int).GetDefaultValue(), Is.EqualTo(0));

        [Test]
        public void GetDefaultValue_ShouldHandleClassTypes() =>
            Assert.IsNull(typeof(List<int>).GetDefaultValue());

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

        [Test]
        public void GetEmptyListMarker_ShouldReturnTheMarkerProperty()
        {
            PropertyInfo marker = typeof(View3).GetProperties().First(prop => prop.GetCustomAttribute<EmptyListMarkerAttribute>() != null);

            Assert.That(typeof(View3).GetEmptyListMarker(), Is.Not.Null);
            Assert.AreSame(typeof(View3).GetEmptyListMarker(), marker);
        }

        [Test]
        public void GetEmptyListMarker_ShouldReturnNullIfNoMarker()
        {
            Assert.That(typeof(object).GetEmptyListMarker(), Is.Null);
        }

        private class BadListItem : ListItem
        {
            [EmptyListMarker]
            [BelongsTo(typeof(object))]
            public int Cica { get; set; }
        }

        [Test]
        public void GetEmptyListMarker_ShouldThrowOnMultipleMarkers() =>
            Assert.Throws<InvalidOperationException>(() => typeof(BadListItem).GetEmptyListMarker(), Resources.MULTIPLE_EMPTY_LIST_MARKER);

        [Test]
        public void HasOwnMethod_ShouldReturnTrueIfTheMethodIsDeclaredOnTheSource()
        {
            Type type = new { }.GetType();

            Assert.True(type.HasOwnMethod("GetHashCode"));
            Assert.True(type.HasOwnMethod("Equals", typeof(object)));
        }

        [Test]
        public void HasOwnMethod_ShouldReturnFalseIfTheMethodIsNoDeclaredOnTheSource()
        {
            Assert.False(typeof(object).HasOwnMethod("NonExisting"));
            Assert.False(typeof(int[]).HasOwnMethod("Equals"));
        }
    }
}
