using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class PermissionMenuItem
    {
        public string Module { get; set; }      // ex: "Preuve"
        public List<string> Actions { get; set; }  // ex: ["Créer", "Modifier"]
    }
}