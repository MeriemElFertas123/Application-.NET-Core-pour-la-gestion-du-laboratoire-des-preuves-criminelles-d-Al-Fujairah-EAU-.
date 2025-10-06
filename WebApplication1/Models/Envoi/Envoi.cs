// Models/Envoi/Envoi.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Envoi
{
    public class Envoi
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AffaireId { get; set; }

        [Required]
        [StringLength(100)]
        public string TypeAnalyseDemandee { get; set; }

        [Required]
        public DateTime DateEnvoiPrevue { get; set; }

        public DateTime DateEnvoiEffective { get; set; }

        public DateTime? DateReception { get; set; }

        [Required]
        [StringLength(50)]
        public string StatutEnvoi { get; set; } // "Préparé", "Envoyé", "Reçu", "Vérifié", "Refusé"

        [StringLength(500)]
        public string Observations { get; set; }

        // Relations
        public virtual ICollection<EnvoiEchantillon> EnvoiEchantillons { get; set; } = new List<EnvoiEchantillon>();

        [ForeignKey("AffaireId")]
        public virtual Affaire Affaire { get; set; }
    }
}