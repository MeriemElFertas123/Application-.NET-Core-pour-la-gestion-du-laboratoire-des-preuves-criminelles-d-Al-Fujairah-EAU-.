using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using WebApplication1.Models;
using WebApplication1.Models.Stockage;

// Models/Analyste.cs - Version simplifiée
namespace WebApplication1.Models
{
    public class Analyste
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string nom { get; set; }

        [Required]
        public string prenom { get; set; }

        [Required]
        public string specialite { get; set; }


        public string statut { get; set; } = "actif";

        public int ChargeActuelle { get; set; } = 0; // Nombre d'échantillons assignés

        [ForeignKey("User")]
        [StringLength(450)]
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }
        // Relations
        public virtual ICollection<Echantillon> EchantillonsAssignees { get; set; }
        public virtual ICollection<RapportAnalyse> rapports { get; set; }
        public virtual ICollection<AttributionAnalyse> AttributionsRecues { get; set; }

        [NotMapped]
        public string NomComplet => $"{prenom} {nom}";

        public Analyste()
        {
            EchantillonsAssignees = new List<Echantillon>();
            rapports = new List<RapportAnalyse>();
            AttributionsRecues = new List<AttributionAnalyse>();
        }
    }
}
