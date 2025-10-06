using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class AttributionViewModel
    {
        [Required(ErrorMessage = "Veuillez sélectionner un utilisateur")]
        public string SelectedUserId { get; set; }

        public SelectList Users { get; set; }

        [Required]
        public List<PermissionItem> AvailablePermissions { get; set; }

        public AttributionViewModel()
        {
            AvailablePermissions = new List<PermissionItem>();
        }

    }

    public class PermissionItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}