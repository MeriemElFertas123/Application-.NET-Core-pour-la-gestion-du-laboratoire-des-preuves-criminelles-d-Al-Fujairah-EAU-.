// Models/Envoi/EnvoiEchantillon.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Models.Envoi
{
    public class EnvoiEchantillon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EnvoiId { get; set; }

        [Required]
        public int EchantillonId { get; set; }

        [Required]
        [StringLength(50)]
        public string CodeQR { get; set; }

        [Required]
        
        public decimal Poids { get; set; }

        [Required]
        [StringLength(200)]
        public string ConditionsStockage { get; set; }

        [StringLength(100)]
        public string Couleur { get; set; }

        [Required]
        [StringLength(100)]
        public string Emballage { get; set; }

        [StringLength(200)]
        public string Emplacement { get; set; }

        [StringLength(1000)]
        public string Observations { get; set; }

        [StringLength(50)]
        public string StatutEchantillon { get; set; } // "Préparé", "Envoyé", "Reçu", "Vérifié", "Conforme", "Non-conforme", "Stocké"

        public DateTime DatePreparation { get; set; }
        public DateTime? DateVerification { get; set; }

        [StringLength(100)]
        public string VerifiePar { get; set; } // ID ou nom du technicien qui a vérifié

        [StringLength(1000)]
        public string NotesVerification { get; set; }

        // Relations
        [ForeignKey("EnvoiId")]
        public virtual Envoi Envoi { get; set; }

        [ForeignKey("EchantillonId")]
        public virtual Echantillon Echantillon { get; set; }
        public DateTime? DateReception { get; internal set; }
    }
}