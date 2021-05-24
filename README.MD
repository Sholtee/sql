[![Build status](https://ci.appveyor.com/api/projects/status/mlssww4bgg11s9lp/branch/master?svg=true)](https://ci.appveyor.com/project/Sholtee/sqlutils/branch/master) ![AppVeyor tests](https://img.shields.io/appveyor/tests/sholtee/sqlutils) [![Coverage Status](https://coveralls.io/repos/github/Sholtee/sqlutils/badge.svg?branch=master)](https://coveralls.io/github/Sholtee/sqlutils?branch=master)

**This is a proof of concept project, meant for playing only**

At the moment this project has two objectives:
1. **Generating bulked queries**:
  ```csharp
  using Solti.Utils.SQL;
  using ServiceStack.OrmLite; // required only for this example
  ...
  public class Table 
  {
    [AutoIncrement, PrimaryKey]
    public int Id { get; set; }
    public string Data { get; set; }
  }
  ...
  using (IDbConnection conn = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider).CreateDbConnection()) 
  {
    conn.Open();
    conn.CreateTable<Table>();

    using (IBulkedDbConnection bulk = conn.CreateBulkedDbConnection()) 
    {
      bulk.Insert(new Table { Data = "cica" });
      bulk.Insert(new Table { Data = "kutya" });

      bulk.Flush();
    }

    Assert.That(conn.Select<Table>().Count, Is.EqualTo(2));
  }
  ```
2. **Querying complex views with a single line of code**
  ```csharp
  using Solti.Utils.SQL;
  ...
  //
  // Database entities
  //

  [DatabaseEntity]
  public class Login 
  {
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }
    [References(typeof(User))]
    public long UserId { get; set; }
    [Required, Unique]
    public string Email { get; set; }
    [Required]
    public string PwHash { get; set; }
  }

  [DatabaseEntity]
  public class User
  {
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }
    [Required]
    public string Name { get; set; }
  }

  [DatabaseEntity]
  public class Post
  {
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }
    [References(typeof(User))]
    public long UserId { get; set; }
    [Required]
    public string Content { get; set; }
  }

  [DatabaseEntity]
  public class UserRole 
  {
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }
    [References(typeof(User))]
    public long UserId { get; set; }
    [References(typeof(Role))]
    public long RoleId { get; set; }
  }

  [DatabaseEntity]
  public class Role 
  {
    [PrimaryKey]
    public long Id { get; set; }
    [Required, Unique]
    public string Name { get; set; }
    public string Description { get; set; }
  }

  [DatabaseEntity]
  public class Ban 
  {
    [PrimaryKey, AutoIncrement]
    public long Id { get; set; }
    [References(typeof(User))]
    public long UserId { get; set; }
    [Required]
    public DateTime UpTo { get; set; }
  }
  ...
  //
  // Views
  //

  [View(@base: typeof(Login))]
  public class LoginView 
  {
    [BelongsTo(typeof(Login))]
    public long Id { get; set; }
    [BelongsTo(typeof(Login))]
    public string PwHash { get; set; }
    public bool VerifyPassword(string password) => throw new NotImplementedException();
  }

  [View]
  public class UserView: User 
  {
    [Wrapped]
    public LoginView Login { get; set; }
    [Wrapped(required: false)]
    public List<Post> Posts { get; set; }
    [BelongsTo(typeof(Role), column: nameof(Role.Name))]
    public List<string> Roles { get; set; }
    [BelongsTo(typeof(Ban), column: nameof(Ban.UpTo), required: false)]
    public DateTime? BannedUpTo { get; set; }
  }
  ...
  //
  // Logic
  //

  List<UserView> users = Connection.Query<UserView>(expr => expr.Where(...) /*optional*/); // this generates only one (!!!) SQL query
  ```
