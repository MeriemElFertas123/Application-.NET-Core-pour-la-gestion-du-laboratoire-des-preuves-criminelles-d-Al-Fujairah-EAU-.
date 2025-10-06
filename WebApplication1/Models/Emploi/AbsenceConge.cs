using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Emploi
{
    public class AbsenceConge
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Personnel")]
        public int PersonnelId { get; set; }

        [Display(Name = "Emploi du temps")]
        public int? EmploiTempsId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Type")]
        public string Type { get; set; } // Conge, Maladie, Formation, Autre

        [Required]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; }

        [Required]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; }

        [StringLength(500)]
        [Display(Name = "Motif")]
        public string Motif { get; set; }

        [Display(Name = "Remplaçant")]
        public int? RemplacantId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Statut")]
        public string Statut { get; set; } = "Pending"; // Pending, Approved, Rejected

        [Display(Name = "Date de demande")]
        public DateTime DateDemande { get; set; } = DateTime.Now;

        // Relations
        [ForeignKey("PersonnelId")]
        public virtual Personnel Personnel { get; set; }

        [ForeignKey("RemplacantId")]
        public virtual Personnel Remplacant { get; set; }

        [ForeignKey("EmploiTempsId")]
        public virtual EmploiTemps EmploiTemps { get; set; }

        // Propriétés calculées
        [NotMapped]
        public string TypeLibelle
        {
            get
            {
                switch (Type)
                {
                    case "Conge": return "Congé";
                    case "Maladie": return "Maladie";
                    case "Formation": return "Formation";
                    case "Autre": return "Autre";
                    default: return "Inconnu";
                }
            }
        }

        [NotMapped]
        public string StatutLibelle
        {
            get
            {
                switch (Statut)
                {
                    case "Pending": return "En attente";
                    case "Approved": return "Approuvé";
                    case "Rejected": return "Refusé";
                    default: return "Inconnu";
                }
            }
        }
    }
}