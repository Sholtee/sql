/********************************************************************************
* QueryTests.cs                                                                 *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using NUnit.Framework;

using ServiceStack.Data;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace Solti.Utils.SQL.OrmLite.Tests
{
    using Interfaces;
    using DatabaseEntity = Interfaces.DataAnnotations.DatabaseEntityAttribute;

    [TestFixture]
    public class QueryTests
    {
        private static readonly IDbConnectionFactory ConnectionFactory = new OrmLiteConnectionFactory(":memory:", SqliteDialect.Provider);

        private IDbConnection FConnection;

        #region Database entities
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
        #endregion

        #region Views
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
        #endregion

        private long RegisterUser(string name, string email, string pwHash, int[] roles) 
        {
            long userId = FConnection.Insert(new User
            {
                Name = name
            }, selectIdentity: true);

            FConnection.Insert(new Login
            {
                UserId = userId,
                Email  = email,
                PwHash = pwHash
            });

            FConnection.InsertAll(roles.Select(roleId => new UserRole { UserId = userId, RoleId = roleId }));

            return userId;
        }

        [OneTimeSetUp]
        public void SetupFixture() 
        {
            Config.Use<OrmLiteConfig>();
            Config.Use(new DiscoveredDataTables(typeof(QueryTests).Assembly));

            FConnection = ConnectionFactory.OpenDbConnection();
            FConnection.DropAndCreateTables(Config.KnownTables.ToArray());

            FConnection.InsertAll(new[]
            {
                new Role
                {
                    Id = 1,
                    Name = "Admin"
                },
                new Role
                {
                    Id = 2,
                    Name = "User"
                }
            });

            RegisterUser("Root", "admin@cica.hu", "xxx", new[] { 1, 2 });

            long userId = RegisterUser("Lajos", "user@cica.hu", "yyy", new[] { 2 });

            FConnection.InsertAll(new[] { "msg1", "msg2" }.Select(msg => new Post 
            {
                UserId = userId,
                Content = msg
            }));

            SmartSqlBuilder<UserView>.Initialize();
        }

        [Test]
        public void Query_ShouldGroup() 
        {
            List<UserView> users = FConnection.Query<UserView>();

            Assert.That(users.Count, Is.EqualTo(2));

            UserView admin = users.Single(u => u.Name == "Root");
            Assert.That(admin.Roles.OrderBy(r => r).SequenceEqual(new[] { "Admin", "User" }));
            Assert.That(admin.Posts, Is.Empty);
            Assert.That(admin.Login.PwHash, Is.EqualTo("xxx"));
            Assert.That(admin.BannedUpTo, Is.Null);

            UserView user = users.Single(u => u.Name == "Lajos");
            Assert.That(user.Roles.Single(), Is.EqualTo("User"));
            Assert.That(user.Posts.OrderBy(p => p.Content).Select(p => p.Content).SequenceEqual(new[] { "msg1", "msg2" }));
            Assert.That(user.Login.PwHash, Is.EqualTo("yyy"));
            Assert.That(user.BannedUpTo, Is.Null);
        }
    }
}
