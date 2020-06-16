/********************************************************************************
* QueryTests.cs                                                                 *
*                                                                               *
* Author: Denes Solti                                                           *
********************************************************************************/
using System;
using System.Collections.Generic;

using NUnit.Framework;
using ServiceStack.DataAnnotations;

namespace Solti.Utils.SQL.OrmLite.Tests
{
    using Interfaces;
    using DatabaseEntityAttribute = Interfaces.DataAnnotations.DatabaseEntityAttribute;

    [TestFixture]
    public class QueryTests
    {
        #region Database entities
        [DatabaseEntity]
        public class Login 
        {
            [PrimaryKey]
            public Guid Id { get; }
            [References(typeof(User))]
            public Guid UserId { get; set; }
            [Required]
            public string Email { get; set; }
            [Required]
            public string PwHash { get; set; }
        }

        [DatabaseEntity]
        public class User 
        {
            [PrimaryKey]
            public Guid Id { get; }
            [Required]
            public string Name { get; set; }
        }

        [DatabaseEntity]
        public class Post
        {
            [PrimaryKey]
            public Guid Id { get; set; }
            [References(typeof(User))]
            public Guid UserId { get; set; }
            [Required]
            public string Content { get; set; }
        }

        [DatabaseEntity]
        public class UserRole 
        {
            [References(typeof(User))]
            public Guid UserId { get; set; }
            [References(typeof(Role))]
            public Guid RoleId { get; set; }
        }

        [DatabaseEntity]
        public class Role 
        {
            [PrimaryKey]
            public Guid Id { get; set; }
            [Required]
            public string Name { get; set; }
            public string Description { get; set; }
        }

        [DatabaseEntity]
        public class Ban 
        {
            [PrimaryKey]
            public Guid Id { get; set; }
            [References(typeof(User))]
            public Guid UserId { get; set; }
            [Required]
            public DateTime Until { get; set; }
        }
        #endregion

        [View(Base = typeof(Login))]
        public class LoginView 
        {
            [BelongsTo(typeof(Login))]
            public string PwHash { get; set; }
            public virtual bool VerifyPassword(string password) => false;
        }

        public class UserView: User 
        {
            [Wrapped]
            public LoginView Login { get; set; }
            [Wrapped]
            public List<Post> Posts { get; set; }
            [BelongsTo(typeof(Role), column: nameof(Role.Name))]
            public List<string> Roles { get; set; }
            [BelongsTo(typeof(Ban), required: false, column: nameof(Ban.Until))]
            public DateTime? BannedUntil { get; set; }
        }
    }
}
