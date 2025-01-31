﻿using System.Collections.Generic;
using System.Security.Claims;

namespace Orchard.Security.Permissions
{
    public class Permission
    {
        public const string ClaimType = "Permission";

        public Permission(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public IEnumerable<Permission> ImpliedBy { get; set; }

        public static implicit operator Claim(Permission p)
        {
            return new Claim(ClaimType, p.Name);
        }
    }
}
