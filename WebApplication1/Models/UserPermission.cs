using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;

public class UserPermission
{
    [Key]
    public int Id { get; set; } // Ajouter une clé primaire
    public string UserId { get; set; }
    public int PermissionId { get; set; }

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; }

    [ForeignKey("PermissionId")]
    public virtual Permission Permission { get; set; }
}
