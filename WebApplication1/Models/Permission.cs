using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApplication1.Models;

namespace WebApplication1.Models
{
public class Permission
{
    public int Id { get; set; }
    public string permission { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; }
    public virtual ICollection<UserPermission> UserPermissions { get; set; }
}

}