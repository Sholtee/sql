/********************************************************************************
* FakeEntities.cs                                                               *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;

namespace Solti.Utils.SQL.Tests
{
    using Interfaces;
    using Interfaces.DataAnnotations;

    [DatabaseEntity]
    public class OrmType
    {
        [PrimaryKey]
        public Guid Id { get; set; }
    }

    [DatabaseEntity]
    public class OrmTypeWithReference
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [References(typeof(OrmType))]
        public string Reference { get; set; }
    }

    //
    // Start_Node <- Node2 -> Goal_Node [LEGROVIDEBB]
    // Node5 -> Start_Node [NEM JO]
    // Goal_Node <- Node6 -> Node7 -> Start_Node [JO, DE TUL HOSSZU]
    // Node8 -> Node7 [NEM JO]
    [DatabaseEntity]
    public class Start_Node  // start
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string ReferenceWithoutAttribute { get; set; }
        [Ignore]
        public string Ignored { get; set; }
    }

    [DatabaseEntity]
    public class Node2
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [References(typeof(Start_Node))]
        public string Reference { get; set; }
        [References(typeof(Goal_Node))]
        public string Reference2 { get; set; }
        public int Foo { get; set; }
    }

    [DatabaseEntity]
    public class Goal_Node  // cel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [References(typeof(Node4))]  // Ez mar lenyegtelen nem kene erre tovabb menni
        public string Reference { get; set; }
    }

    [DatabaseEntity]
    public class Node4
    {
        [PrimaryKey]
        public Guid Id { get; set; }
    }

    [DatabaseEntity]
    public class Node5
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [References(typeof(Start_Node))]
        public string Reference { get; set; }
    }

    [DatabaseEntity]
    public class Node6
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [References(typeof(Node7))]
        public string Reference { get; set; }
        [References(typeof(Goal_Node))]
        public string Reference2 { get; set; }
    }

    [DatabaseEntity]
    public class Node7
    {
        [PrimaryKey]
        public virtual Guid Id { get; set; }
        [References(typeof(Start_Node))]
        public Guid Reference { get; set; }
    }

    [DatabaseEntity]
    public class Node8
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [References(typeof(Node7))]
        public string Reference { get; set; }
    }

    public class View1
    {
        [BelongsTo(typeof(Start_Node), alias: "ReferenceWithoutAttribute")]
        public string SimpleColumnSelection { get; set; }
        [BelongsTo(typeof(Goal_Node), alias: "Id")]
        public string IdSelection { get; set; }
        [BelongsTo(typeof(Node2), alias: "Foo")] // mar join-olva lett a GOAL_NODE-nal
        public int SelectionFromAlreadyJoinedTable { get; set; }
        [BelongsTo(typeof(Node5), alias: "Id", required: false)]
        public string SelectionFromAnotherRoute { get; set; }
        [BelongsTo(typeof(Node5), alias: "Reference", required: false)]
        public string SecondSelectionFromAnotherRoute { get; set; }
    }

    public class View2
    {
        [CountOf(typeof(Node2), alias: "Id")]
        public int Count { get; set; }
        [BelongsTo(typeof(Start_Node), alias: "ReferenceWithoutAttribute")]
        public int Foo { get; set; }
        public int Ignored { get; set; }
    }

    public class View3
    {
        [EmptyListMarker]
        [BelongsTo(typeof(Goal_Node), order: Order.Ascending)]
        public string Id { get; set; }
        [CountOf(typeof(Node2), alias: "Id")]
        public int Count { get; set; }
    }

    public class WrappedView1_Bad
    {
        [BelongsTo(typeof(Start_Node))]
        public int Id { get; set; }
        [Wrapped]
        public List<View3> ViewList { get; set; }
    }

    public class WrappedView1
    {
        [BelongsTo(typeof(Start_Node), alias: "Id")]
        public int Azonosito { get; set; }
        [Wrapped]
        public List<View3> ViewList { get; set; }
    }

    public class WrappedView2
    {
        [BelongsTo(typeof(Start_Node), alias: "Id")]
        public int Azonosito { get; set; }
        [Wrapped]
        public List<View1> ViewList { get; set; }
        [Wrapped]
        public List<View2> ViewList2 { get; set; }
    }

    public class WrappedView3
    {
        [BelongsTo(typeof(Start_Node), alias: "Id")]
        public int Azonosito { get; set; }
        [Wrapped]
        public View3 View { get; set; }
    }

    public class WrappedView3_Extesnion
    {
        [BelongsTo(typeof(Start_Node), alias: "Id")]
        public int Azonosito { get; set; }
        [Wrapped]
        public List<Extension1> ViewList { get; set; }
    }

    public class WrappedView4_Complex
    {
        [BelongsTo(typeof(Start_Node), alias: "Id")]
        public int NagyonId { get; set; }
        [Wrapped]
        public List<WrappedView1> AnotherViewList { get; set; }
    }

    public class Node7_View : Node7 // Ez kell h a kicsomagolo megtalalja
    {
        [Ignore]  // Ne legyen property nev utkozes
        public override Guid Id { get; set; }

        [BelongsTo(typeof(Node7), alias: "Id")] // Megoldas h az Id-ben a korrekt ertek legyen
        public Guid Azonosito
        {
            set { Id = value; }
        }
    }

    public class Start_Node_View : Start_Node
    {
        [Wrapped]
        public List<Node7_View> Children { get; set; }
    }

    public class Extension1 : Start_Node
    {
        [BelongsTo(typeof(Goal_Node), alias: "Id")]
        public string IdSelection { get; set; }
    }

    public class Extension2 : Start_Node
    {
        [BelongsTo(typeof(Goal_Node), alias: "Id", order: Order.Ascending)]
        public string AnotherId { get; set; }
        [CountOf(typeof(Node2), alias: "Id")]
        public int Count { get; set; }
    }
}
