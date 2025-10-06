using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
namespace WebApplication1.Models.Emploi
{
    public class Personnel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Type de personnel")]
        public string TypePersonnel { get; set; } // Technician, Analyst, Specialist, Expert

        [Required]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Téléphone")]
        public string Telephone { get; set; }

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;

        [Display(Name = "Date d'embauche")]
        public DateTime DateEmbauche { get; set; }

        // Relations
        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }
        public virtual ICollection<AffectationPersonnel> Affectations { get; set; } = new List<AffectationPersonnel>();
        public virtual ICollection<AbsenceConge> AbsencesConges { get; set; } = new List<AbsenceConge>();

        // Propriétés calculées
        [NotMapped]
        public string NomComplet => $"Dr. {Nom}";

        [NotMapped]
        public string TypePersonnelLibelle
        {
            get
            {
                switch (TypePersonnel)
                {
                    case "Technician": return "Tech";
                    case "Analyst": return "Ana";
                    case "Specialist": return "Spe";
                    case "Expert": return "Exp";
                    default: return "N/A";
                }
            }
        }
    }
}