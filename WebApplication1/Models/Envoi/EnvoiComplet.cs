using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Models.Envoi
{
    public class EnvoiComplet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AffaireId { get; set; }

        [Required]
        [StringLength(100)]
        public string TypeAnalyseDemandee { get; set; } = string.Empty;

        [Required]
        public DateTime DateEnvoiPrevue { get; set; }

        public DateTime DateEnvoiEffective { get; set; }

        [Required]
        [StringLength(50)]
        public string StatutEnvoi { get; set; } = string.Empty;

        [StringLength(500)]
        public string ObservationsEnvoi { get; set; } = string.Empty;

        // Informations de l'échantillon
        [Required]
        public int EchantillonId { get; set; }

        [Required]
        [StringLength(50)]
        public string CodeQR { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Poids { get; set; }

        [Required]
        [StringLength(200)]
        public string ConditionsStockage { get; set; } = string.Empty;

        [StringLength(100)]
        public string Couleur { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Emballage { get; set; } = string.Empty;

        [StringLength(200)]
        public string Emplacement { get; set; } = string.Empty;

        [StringLength(1000)]
        public string ObservationsEchantillon { get; set; } = string.Empty;

        [StringLength(50)]
        public string StatutEchantillon { get; set; } = string.Empty;

        public DateTime DatePreparation { get; set; }
        public DateTime? DateReception { get; set; }
        public DateTime? DateVerification { get; set; }

        [StringLength(100)]
        public string VerifiePar { get; set; } = string.Empty;

        [StringLength(1000)]
        public string NotesVerification { get; set; } = string.Empty;

        // Relations
        [ForeignKey("AffaireId")]
        public virtual Affaire Affaire { get; set; } = null!;

        public virtual ICollection<Echantillon> Echantillons { get; set; } = new List<Echantillon>();

        [NotMapped]
        public int NombreEchantillons => Echantillons?.Count ?? 0;
    }
}