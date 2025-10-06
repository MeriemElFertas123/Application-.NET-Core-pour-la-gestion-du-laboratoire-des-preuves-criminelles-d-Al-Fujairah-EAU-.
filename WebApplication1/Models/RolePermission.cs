using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class RolePermission
    {
        public int Id { get; set; }

        public string RoleId { get; set; } // AspNetRoles.Id
        public int DroitId { get; set; }

        public virtual IdentityRole Role { get; set; }
        public virtual Permission Permission { get; set; }
        public string PermissionId { get; internal set; }
    }
}