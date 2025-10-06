using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    [Table("HistoriqueAffaires")]
    public class HistoriqueAffaire
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Affaire")]
        public int AffaireId { get; set; }
        public virtual Affaire Affaire { get; set; }

        [Required]
        [StringLength(200)]
        public string Action { get; set; }

        [StringLength(100)]
        public string MotifCloture { get; set; }

        [StringLength(1000)]
        public string Commentaire { get; set; }

        [Required]
        public DateTime DateAction { get; set; } = DateTime.Now;

        [Required]
        [StringLength(450)] // Taille standard pour les IDs ASP.NET Identity
        public string UserId { get; set; }

        [StringLength(100)]
        public string NomUtilisateur { get; set; }

        // Métadonnées supplémentaires
        [StringLength(50)]
        public string TypeAction { get; set; } // "Cloture", "Modification", "Commentaire", etc.

        [StringLength(200)]
        public string DetailsTechniques { get; set; } // Pour stocker des infos techniques si nécessaire
    }

    // Enum pour les types d'actions
    public enum TypeActionAffaire
    {
        Ouverture,
        Modification,
        Suspension,
        Reprise,
        Resolution,
        ClotureDefinitive,
        Archivage,
        Commentaire
    }
}