// Models/Envoi/RapportNonConformite.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Envoi
{
    public class RapportNonConformite
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EnvoiEchantillonId { get; set; }

        [Required]
        [StringLength(100)]
        public string NumeroRapport { get; set; }

        [Required]
        public DateTime DateRapport { get; set; }

        [Required]
        [StringLength(100)]
        public string RedacteurRapport { get; set; }

        [Required]
        [StringLength(2000)]
        public string DescriptionNonConformite { get; set; }

        [StringLength(1000)]
        public string CausesProbables { get; set; }

        [Required]
        [StringLength(100)]
        public string ActionRecommandee { get; set; } // "Retour", "Contact", "Archive"

        [StringLength(2000)]
        public string NotesDetaillees { get; set; }

        [StringLength(50)]
        public string StatutRapport { get; set; } // "En cours", "Validé", "Traité"

        // Relations
        [ForeignKey("EnvoiCompletId")]
        public virtual EnvoiComplet EnvoiComplet { get; set; }
    }
}