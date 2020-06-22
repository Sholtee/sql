/********************************************************************************
* WrapperTests.cs                                                               *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections;
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
    public sealed class WrapperTests
    {
        [SetUp]
        public void Setup() => Config.Use(new DisoveredDataTables(typeof(WrapperTests).Assembly));

        [Test]
        public void UnwrappedView_ShouldUnwrapSimpleViews()
        {
            Type unwrapped = Unwrapped<View2>.Type;

            typeof(View2)
                .GetProperties()
                .ForEach(prop =>
                {
                    ColumnSelectionAttribute originalAttr = prop.GetCustomAttribute<ColumnSelectionAttribute>();
                    PropertyInfo queried = unwrapped.GetProperty(prop.Name);

                    if (originalAttr == null)
                        Assert.That(queried, Is.Null);
                    else
                    {
                        Assert.That(queried, Is.Not.Null);
                        Assert.That(queried.PropertyType, Is.EqualTo(prop.PropertyType));

                        ColumnSelectionAttribute queriedAttr = queried.GetCustomAttribute<ColumnSelectionAttribute>();

                        Assert.That(originalAttr.GetType(), Is.EqualTo(queriedAttr.GetType()));
                        originalAttr
                            .GetType()
                            .GetProperties()
                            .ForEach(p => Assert.That(p.FastGetValue(originalAttr), Is.EqualTo(p.FastGetValue(queriedAttr))));
                    }
                });
        }

        [Test]
        public void UnwrappedView_ShouldUnwrapSimpleViewsDescendingFromDataTable()
        {
            Type unwrapped = Unwrapped<Extension1>.Type;

            typeof(Extension1)
                .GetProperties()
                .ForEach(prop =>
                {
                    PropertyInfo queried = unwrapped.GetProperty(prop.Name);
                    if (prop.GetCustomAttribute<IgnoreAttribute>() != null)
                        Assert.That(queried, Is.Null);
                    else
                    {
                        Assert.That(queried, Is.Not.Null);
                        Assert.That(queried.PropertyType, Is.EqualTo(prop.PropertyType));

                        ColumnSelectionAttribute
                            originalAttr = prop.GetCustomAttribute<ColumnSelectionAttribute>(),
                            queriedAttr  = queried.GetCustomAttribute<ColumnSelectionAttribute>();

                        if (originalAttr == null)
                        {
                            Assert.That(queriedAttr.GetType(), Is.EqualTo(typeof(BelongsToAttribute)));
                            Assert.That(queriedAttr.OrmType, Is.EqualTo(typeof(Extension1).GetBaseDataTable()));
                        }
                        else
                        {
                            Assert.That(originalAttr.GetType(), Is.EqualTo(queriedAttr.GetType()));
                            originalAttr
                                .GetType()
                                .GetProperties()
                                .ForEach(p => Assert.That(p.FastGetValue(originalAttr), Is.EqualTo(p.FastGetValue(queriedAttr))));
                        }
                    }
                });
        }

        [Test]
        public void UnwrappedView_ShouldCache() => Assert.AreSame(Unwrapped<View2>.Type, Unwrapped<View2>.Type);

        [TestCase(typeof(WrappedView1), nameof(WrappedView1.ViewList))]
        [TestCase(typeof(WrappedView3), nameof(WrappedView3.View))]
        public void UnwrappedView_ShouldUnwrapComplexViews(Type view, string wrappedProp)
        {
            Type unwrapped = (Type) typeof(Unwrapped<>)
                .MakeGenericType(view)
                .GetProperty(nameof(Type))
                .GetValue(null);

            view
                .GetProperties()
                .Where(prop => prop.GetCustomAttribute<WrappedAttribute>() == null)
                .Concat(new WrappedSelection(view.GetProperty(wrappedProp)).UnderlyingType.GetProperties())
                .ForEach(prop =>
                {
                    ColumnSelectionAttribute originalAttr = prop.GetCustomAttribute<ColumnSelectionAttribute>();
                    PropertyInfo queried = unwrapped.GetProperty(prop.Name);

                    if (originalAttr == null)
                        Assert.That(queried, Is.Null);
                    else
                    {
                        Assert.That(queried, Is.Not.Null);
                        Assert.That(queried.PropertyType, Is.EqualTo(prop.PropertyType));

                        ColumnSelectionAttribute queriedAttr = queried.GetCustomAttribute<ColumnSelectionAttribute>();

                        Assert.That(originalAttr.GetType(), Is.EqualTo(queriedAttr.GetType()));
                        originalAttr
                            .GetType()
                            .GetProperties()
                            .ForEach(p => Assert.That(p.FastGetValue(originalAttr), Is.EqualTo(p.FastGetValue(queriedAttr))));
                    }
                });
        }

        [Test]
        public void UnwrappedView_ShouldUnwrapMoreComplexViewHavingMultipleWrappedProperties()
        {
            Type unwrapped = Unwrapped<WrappedView2>.Type;

            typeof(WrappedView2)
                .GetProperties()
                .Where(prop => prop.GetCustomAttribute<WrappedAttribute>() == null)
                .Concat(typeof(WrappedView2).GetProperty(nameof(WrappedView2.ViewList)).PropertyType.GetGenericArguments().Single().GetProperties())
                .Concat(typeof(WrappedView2).GetProperty(nameof(WrappedView2.ViewList2)).PropertyType.GetGenericArguments().Single().GetProperties())
                .ForEach(prop =>
                {
                    ColumnSelectionAttribute originalAttr = prop.GetCustomAttribute<ColumnSelectionAttribute>();
                    PropertyInfo queried = unwrapped.GetProperty(prop.Name);

                    if (originalAttr == null)
                        Assert.That(queried, Is.Null);
                    else
                    {
                        Assert.That(queried, Is.Not.Null);
                        Assert.That(queried.PropertyType, Is.EqualTo(prop.PropertyType));

                        ColumnSelectionAttribute queriedAttr = queried.GetCustomAttribute<ColumnSelectionAttribute>();

                        Assert.That(originalAttr.GetType(), Is.EqualTo(queriedAttr.GetType()));
                        originalAttr
                            .GetType()
                            .GetProperties()
                            .ForEach(p => Assert.That(p.FastGetValue(originalAttr), Is.EqualTo(p.FastGetValue(queriedAttr))));
                    }
                });
        }

        [Test]
        public void UnwrappedView_ShouldUnwrapViewsHavingWrappedPropertyWithTypeOfDataTableDescendant()
        {
            Type unwrapped = Unwrapped<WrappedView3_Extesnion>.Type;

            typeof(Extension1)
                .GetProperties()
                .Concat(typeof(WrappedView3_Extesnion).GetProperty(nameof(WrappedView3_Extesnion.ViewList)).PropertyType.GetGenericArguments().Single().GetProperties())
                .ForEach(prop =>
                {
                    PropertyInfo queried = unwrapped.GetProperty(prop.Name);
                    if (prop.GetCustomAttribute<IgnoreAttribute>() != null)
                        Assert.That(queried, Is.Null);
                    else
                    {
                        Assert.That(queried, Is.Not.Null);
                        Assert.That(queried.PropertyType, Is.EqualTo(prop.PropertyType));

                        ColumnSelectionAttribute
                            originalAttr = prop.GetCustomAttribute<ColumnSelectionAttribute>(),
                            queriedAttr  = queried.GetCustomAttribute<ColumnSelectionAttribute>();

                        if (originalAttr == null)
                        {
                            Assert.That(queriedAttr.GetType(), Is.EqualTo(typeof(BelongsToAttribute)));
                            Assert.That(queriedAttr.OrmType, Is.EqualTo(typeof(Extension1).GetBaseDataTable()));
                        }
                        else
                        {
                            Assert.That(originalAttr.GetType(), Is.EqualTo(queriedAttr.GetType()));
                            originalAttr
                                .GetType()
                                .GetProperties()
                                .ForEach(p => Assert.That(p.FastGetValue(originalAttr), Is.EqualTo(p.FastGetValue(queriedAttr))));
                        }
                    }
                });
        }

        [Test]
        public void UnwrappedView_ShouldBeRecursive()
        {
            Type unwrapped = Unwrapped<WrappedView4_Complex>.Type;

            typeof(WrappedView4_Complex)
                .GetProperties()
                .Where(prop => prop.GetCustomAttribute<WrappedAttribute>() == null)
                .Concat(typeof(WrappedView1).GetProperties().Where(prop => prop.GetCustomAttribute<WrappedAttribute>() == null))
                .Concat(typeof(View3).GetProperties())
                .ForEach(prop =>
                {
                    ColumnSelectionAttribute originalAttr = prop.GetCustomAttribute<ColumnSelectionAttribute>();
                    PropertyInfo queried = unwrapped.GetProperty(prop.Name);

                    Assert.That(queried, Is.Not.Null);
                    Assert.That(queried.PropertyType, Is.EqualTo(prop.PropertyType));

                    ColumnSelectionAttribute queriedAttr = queried.GetCustomAttribute<ColumnSelectionAttribute>();

                    Assert.That(originalAttr.GetType(), Is.EqualTo(queriedAttr.GetType()));
                    originalAttr
                        .GetType()
                        .GetProperties()
                        .ForEach(p => Assert.That(p.FastGetValue(originalAttr), Is.EqualTo(p.FastGetValue(queriedAttr))));
                });
        }

        [Test]
        public void UnwrappedView_ShouldHandlePropertyNameCollisions()
        {
            Type unwrapped = null;
            Assert.DoesNotThrow(() => unwrapped = Unwrapped<CollidingWrappedView>.Type);

            Assert.That(unwrapped.GetProperty("Id_0"), Is.Not.Null);
            Assert.That(unwrapped.GetProperty("Id_0").GetCustomAttribute<MapToAttribute>()?.Property, Is.EqualTo("Id"));

            Assert.That(unwrapped.GetProperty("Id_1"), Is.Not.Null);
            Assert.That(unwrapped.GetProperty("Id_1").GetCustomAttribute<MapToAttribute>()?.Property, Is.EqualTo("Id"));
        }

        [Test]
        public void Wrapper_ShouldWorkWithViewsWithoutListProperty()
        {
            Type unwrapped = Unwrapped<View3 /*Nincs benn lista tulajdonsag*/>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);
            objs.Add(unwrapped.MakeInstance()
                .Set("Id", 1.ToString())
                .Set("Count", 10));

            List<View3> result = Wrapper.Wrap<View3>(objs);

            Assert.That(result.Count, Is.EqualTo(objs.Count));
            Assert.That(result[0].Id, Is.EqualTo("1"));
            Assert.That(result[0].Count, Is.EqualTo(10));
        }

        [Test]
        public void Wrapper_ShouldWorkWithComplexViews()
        {
            Type unwrapped = Unwrapped<WrappedView1>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 1)
                .Set("Id", 1.ToString())
                .Set("Count", 10));
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 1)
                .Set("Id", 2.ToString())
                .Set("Count", 15));
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 2)
                .Set("Id", 2.ToString()) // DIREKT 2 megint
                .Set("Count", 20));

            List<WrappedView1> result = Wrapper.Wrap<WrappedView1>(objs);

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].Azonosito, Is.EqualTo(1));
            Assert.That(result[0].ViewList, Is.Not.Null);
            Assert.That(result[0].ViewList.Count, Is.EqualTo(2));
            Assert.That(result[0].ViewList[0].Id, Is.EqualTo("1"));
            Assert.That(result[0].ViewList[0].Count, Is.EqualTo(10));
            Assert.That(result[0].ViewList[1].Id, Is.EqualTo("2"));
            Assert.That(result[0].ViewList[1].Count, Is.EqualTo(15));

            Assert.That(result[1].Azonosito, Is.EqualTo(2));
            Assert.That(result[1].ViewList, Is.Not.Null);
            Assert.That(result[1].ViewList.Count, Is.EqualTo(1));
            Assert.That(result[1].ViewList[0].Id, Is.EqualTo("2"));
            Assert.That(result[1].ViewList[0].Count, Is.EqualTo(20));
        }

        [Test]
        public void Wrapper_ShouldTakeEmptyListMarkerAttributeIntoAccount()
        {
            Type unwrapped = Unwrapped<WrappedView1>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 1));

            List<WrappedView1> result = Wrapper.Wrap<WrappedView1>(objs);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Azonosito, Is.EqualTo(1));
            Assert.That(result[0].ViewList, Is.Not.Null);
            Assert.That(result[0].ViewList, Is.Empty);
        }

        [Test]
        public void Wrapper_ShouldWorkWithMoreComplexViews()
        {
            Type unwrapped = Unwrapped<WrappedView2>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);

            Guid id = Guid.NewGuid();

            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 1)
                .Set("Id", id)
                .Set("IdSelection", 1.ToString())
                .Set("SimpleColumnSelection", "xyz")
                .Set("Foo", 10)
                .Set("Count", 15));
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 1)
                .Set("Id", id)
                .Set("IdSelection", 2.ToString())
                .Set("SimpleColumnSelection", "abc")
                .Set("Foo", 10)
                .Set("Count", 15));

            id = Guid.NewGuid();

            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 2)
                .Set("Id", id)
                .Set("IdSelection", 2.ToString())
                .Set("SimpleColumnSelection", "zyx")
                .Set("Foo", 20)
                .Set("Count", 25));
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 2)
                .Set("Id", id)
                .Set("IdSelection", 2.ToString())
                .Set("SimpleColumnSelection", "zyx")
                .Set("Foo", 30)
                .Set("Count", 35));

            List<WrappedView2> result = Wrapper.Wrap<WrappedView2>(objs);

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].Azonosito, Is.EqualTo(1));
            Assert.That(result[0].ViewList.Count, Is.EqualTo(2));
            Assert.That(result[0].ViewList[0].IdSelection, Is.EqualTo("1"));
            Assert.That(result[0].ViewList[0].SimpleColumnSelection, Is.EqualTo("xyz"));
            Assert.That(result[0].ViewList[1].IdSelection, Is.EqualTo("2"));
            Assert.That(result[0].ViewList[1].SimpleColumnSelection, Is.EqualTo("abc"));
            Assert.That(result[0].ViewList2.Count, Is.EqualTo(1));
            Assert.That(result[0].ViewList2[0].Foo, Is.EqualTo(10));
            Assert.That(result[0].ViewList2[0].Count, Is.EqualTo(15));

            Assert.That(result[1].Azonosito, Is.EqualTo(2));
            Assert.That(result[1].ViewList.Count, Is.EqualTo(1));
            Assert.That(result[1].ViewList[0].IdSelection, Is.EqualTo("2"));
            Assert.That(result[1].ViewList[0].SimpleColumnSelection, Is.EqualTo("zyx"));
            Assert.That(result[1].ViewList2.Count, Is.EqualTo(2));
            Assert.That(result[1].ViewList2[0].Foo, Is.EqualTo(20));
            Assert.That(result[1].ViewList2[0].Count, Is.EqualTo(25));
            Assert.That(result[1].ViewList2[1].Foo, Is.EqualTo(30));
            Assert.That(result[1].ViewList2[1].Count, Is.EqualTo(35));
        }

        [Test]
        public void Wrapper_ShouldWorkWithViewsDescendingFromOrmType()
        {
            Type unwrapped = Unwrapped<WrappedView3_Extesnion>.Type;

            Guid
                id1 = Guid.NewGuid(),
                id2 = Guid.NewGuid();

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 1)
                .Set("Id", id1)
                .Set("IdSelection", "cica"));
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 1)
                .Set("Id", id2)
                .Set("IdSelection", "kutya"));

            List<WrappedView3_Extesnion> result = Wrapper.Wrap<WrappedView3_Extesnion>(objs);

            Assert.That(result.Count, Is.EqualTo(1));

            Assert.That(result[0].Azonosito, Is.EqualTo(1));
            Assert.That(result[0].ViewList, Is.Not.Null);
            Assert.That(result[0].ViewList.Count, Is.EqualTo(2));
            Assert.That(result[0].ViewList[0].Id, Is.EqualTo(id1));
            Assert.That(result[0].ViewList[0].IdSelection, Is.EqualTo("cica"));
            Assert.That(result[0].ViewList[1].Id, Is.EqualTo(id2));
            Assert.That(result[0].ViewList[1].IdSelection, Is.EqualTo("kutya"));
        }

        [Test]
        public void Wrapper_ShouldBeRecursive()
        {
            Type unwrapped = Unwrapped<WrappedView4_Complex>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);
            objs.Add(unwrapped.MakeInstance()
                .Set("NagyonId", 1)
                .Set("Azonosito", 2)
                .Set("Id", 1.ToString())
                .Set("Count", 10));
            objs.Add(unwrapped.MakeInstance()
                .Set("NagyonId", 1)
                .Set("Azonosito", 2)
                .Set("Id", 2.ToString())
                .Set("Count", 20));
            objs.Add(unwrapped.MakeInstance()
                .Set("NagyonId", 1)
                .Set("Azonosito", 3)
                .Set("Id", 1.ToString())
                .Set("Count", 50));
            objs.Add(unwrapped.MakeInstance()
                .Set("NagyonId", 2)
                .Set("Azonosito", 3)
                .Set("Id", 4.ToString())
                .Set("Count", 100));
            objs.Add(unwrapped.MakeInstance()
                .Set("NagyonId", 2)
                .Set("Azonosito", 3)
                .Set("Id", 5.ToString())
                .Set("Count", 200));

            List<WrappedView4_Complex> result = Wrapper.Wrap<WrappedView4_Complex>(objs);

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].NagyonId, Is.EqualTo(1));
            Assert.That(result[0].AnotherViewList.Count, Is.EqualTo(2));
            Assert.That(result[0].AnotherViewList[0].Azonosito, Is.EqualTo(2));
            Assert.That(result[0].AnotherViewList[0].ViewList.Count, Is.EqualTo(2));
            Assert.That(result[0].AnotherViewList[0].ViewList[0].Id, Is.EqualTo(1.ToString()));
            Assert.That(result[0].AnotherViewList[0].ViewList[0].Count, Is.EqualTo(10));
            Assert.That(result[0].AnotherViewList[0].ViewList[1].Id, Is.EqualTo(2.ToString()));
            Assert.That(result[0].AnotherViewList[0].ViewList[1].Count, Is.EqualTo(20));
            Assert.That(result[0].AnotherViewList[1].Azonosito, Is.EqualTo(3));
            Assert.That(result[0].AnotherViewList[1].ViewList.Count, Is.EqualTo(1));
            Assert.That(result[0].AnotherViewList[1].ViewList[0].Id, Is.EqualTo(1.ToString()));
            Assert.That(result[0].AnotherViewList[1].ViewList[0].Count, Is.EqualTo(50));
            Assert.That(result[1].NagyonId, Is.EqualTo(2));
            Assert.That(result[1].AnotherViewList.Count, Is.EqualTo(1));
            Assert.That(result[1].AnotherViewList[0].Azonosito, Is.EqualTo(3));
            Assert.That(result[1].AnotherViewList[0].ViewList.Count, Is.EqualTo(2));
            Assert.That(result[1].AnotherViewList[0].ViewList[0].Id, Is.EqualTo(4.ToString()));
            Assert.That(result[1].AnotherViewList[0].ViewList[0].Count, Is.EqualTo(100));
            Assert.That(result[1].AnotherViewList[0].ViewList[1].Id, Is.EqualTo(5.ToString()));
            Assert.That(result[1].AnotherViewList[0].ViewList[1].Count, Is.EqualTo(200));
        }

        [Test]
        public void Wrapper_ShouldWorkWithViewsHavingNonListWrappedProperty() 
        {
            Type unwrapped = Unwrapped<WrappedView3>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 2)
                .Set("Id", 1.ToString())
                .Set("Count", 10));
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 1)
                .Set("Id", 2.ToString())
                .Set("Count", 0));

            List<WrappedView3> result = Wrapper.Wrap<WrappedView3>(objs);

            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].Azonosito, Is.EqualTo(2));
            Assert.That(result[0].View.Id, Is.EqualTo("1"));
            Assert.That(result[0].View.Count, Is.EqualTo(10));
            Assert.That(result[1].Azonosito, Is.EqualTo(1));
            Assert.That(result[1].View.Id, Is.EqualTo("2"));
            Assert.That(result[1].View.Count, Is.EqualTo(0));
        }

        [Test]
        public void Wrapper_ShouldValidateTheSourceList() 
        {
            Type unwrapped = Unwrapped<WrappedView1>.Type;

            Assert.Throws<ArgumentException>(() => Wrapper.Wrap<WrappedView1>(Array.CreateInstance(unwrapped, 0)), Resources.NOT_A_LIST);
            Assert.Throws<ArgumentException>(() => Wrapper.Wrap<WrappedView1>(new List<object>()), Resources.INCOMPATIBLE_LIST);
        }

        [Test]
        public void Wrapper_ShouldThrowIfTheViewIsAmbiguous() 
        {
            Type unwrapped = Unwrapped<WrappedView3>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 2)
                .Set("Id", 1.ToString())
                .Set("Count", 10));
            objs.Add(unwrapped.MakeInstance()
                .Set("Azonosito", 2)
                .Set("Id", 1.ToString())
                .Set("Count", 0));

            Assert.Throws<InvalidOperationException>(() => Wrapper.Wrap<WrappedView3>(objs), Resources.AMBIGUOUS_RESULT);
        }

        [Test]
        public void Wrapper_ShouldWorkWithValueLists() 
        {
            Type unwrapped = Unwrapped<Start_Node_View_ValueList>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);

            Guid id1 = Guid.NewGuid();

            objs.Add(unwrapped.MakeInstance()
                .Set("Id_0", id1)
                .Set("Id_1", Guid.NewGuid())
                .Set("Reference", 1.ToString()));

            Guid id2 = Guid.NewGuid();

            objs.Add(unwrapped.MakeInstance()
                .Set("Id_0", id2)
                .Set("Id_1", Guid.NewGuid())
                .Set("Reference", 1.ToString()));
            objs.Add(unwrapped.MakeInstance()
                .Set("Id_0", id2)
                .Set("Id_1", Guid.NewGuid())
                .Set("Reference", 2.ToString()));

            List<Start_Node_View_ValueList> result = Wrapper.Wrap<Start_Node_View_ValueList>(objs);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].References.Count, Is.EqualTo(1));
            Assert.That(result[0].References[0], Is.EqualTo(1.ToString()));
            Assert.That(result[1].References.Count, Is.EqualTo(2));
            Assert.That(result[1].References[0], Is.EqualTo(1.ToString()));
            Assert.That(result[1].References[1], Is.EqualTo(2.ToString()));
        }

        [Test]
        public void Wrapper_ShouldHandleEmptyValueLists()
        {
            Type unwrapped = Unwrapped<Start_Node_View_ValueList>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);

            Guid id1 = Guid.NewGuid();

            objs.Add(unwrapped.MakeInstance().Set("Id_0", id1));

            Guid id2 = Guid.NewGuid();

            objs.Add(unwrapped.MakeInstance()
                .Set("Id_0", id2)
                .Set("Id_1", Guid.NewGuid())
                .Set("Reference", 1.ToString()));
            objs.Add(unwrapped.MakeInstance()
                .Set("Id_0", id2)
                .Set("Id_1", Guid.NewGuid())
                .Set("Reference", 2.ToString()));

            List<Start_Node_View_ValueList> result = Wrapper.Wrap<Start_Node_View_ValueList>(objs);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].References, Is.Empty);
            Assert.That(result[1].References.Count, Is.EqualTo(2));
            Assert.That(result[1].References[0], Is.EqualTo(1.ToString()));
            Assert.That(result[1].References[1], Is.EqualTo(2.ToString()));
        }

        [Test]
        public void Wrapper_ShouldHandlePropertyNameCollision() 
        {
            Type unwrapped = Unwrapped<CollidingWrappedView>.Type;

            var objs = (IList) typeof(List<>).MakeInstance(unwrapped);

            objs.Add(unwrapped.MakeInstance()
                .Set("Id_0", 1)
                .Set("Id_1", "kutya")
                .Set("Count", 10));

            objs.Add(unwrapped.MakeInstance()
                .Set("Id_0", 2)
                .Set("Id_1", "cica")
                .Set("Count", 20));
            objs.Add(unwrapped.MakeInstance()
                .Set("Id_0", 2)
                .Set("Id_1", "meresi hiba")
                .Set("Count", 30));

            List<CollidingWrappedView> result = Wrapper.Wrap<CollidingWrappedView>(objs);

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Id, Is.EqualTo(1));
            Assert.That(result[0].ViewList.Count, Is.EqualTo(1));
            Assert.That(result[0].ViewList[0].Id, Is.EqualTo("kutya"));
            Assert.That(result[0].ViewList[0].Count, Is.EqualTo(10));
            Assert.That(result[1].Id, Is.EqualTo(2));
            Assert.That(result[1].ViewList.Count, Is.EqualTo(2));
            Assert.That(result[1].ViewList[0].Id, Is.EqualTo("cica"));
            Assert.That(result[1].ViewList[0].Count, Is.EqualTo(20));
            Assert.That(result[1].ViewList[1].Id, Is.EqualTo("meresi hiba"));
            Assert.That(result[1].ViewList[1].Count, Is.EqualTo(30));
        }
    }
}
