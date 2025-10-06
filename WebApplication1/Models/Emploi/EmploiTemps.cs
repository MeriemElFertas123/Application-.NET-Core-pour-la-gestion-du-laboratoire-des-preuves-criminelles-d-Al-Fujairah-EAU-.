using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Emploi
{
    public class EmploiTemps
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Numéro de semaine")]
        public int NumeroSemaine { get; set; }

        [Required]
        [Display(Name = "Année")]
        public int Annee { get; set; }

        [Required]
        [Display(Name = "Date de début")]
        public DateTime DateDebut { get; set; }

        [Required]
        [Display(Name = "Date de fin")]
        public DateTime DateFin { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Statut")]
        public string Statut { get; set; } = "Draft"; // Draft, Pending, Approved, Archived, Cancelled

        [StringLength(1000)]
        [Display(Name = "Commentaires")]
        public string Commentaires { get; set; }

        [Required]
        [Display(Name = "Date de création")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Display(Name = "Date de modification")]
        public DateTime? DateModification { get; set; }

        [Display(Name = "Date d'approbation")]
        public DateTime? DateApprobation { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Créé par")]
        public string CreePar { get; set; }

        [StringLength(100)]
        [Display(Name = "Approuvé par")]
        public string ApprouvePar { get; set; }

        [StringLength(100)]
        [Display(Name = "Modifié par")]
        public string ModifiePar { get; set; }

        // Relations
        public virtual ICollection<AffectationPersonnel> Affectations { get; set; } = new List<AffectationPersonnel>();
        public virtual ICollection<AbsenceConge> AbsencesConges { get; set; } = new List<AbsenceConge>();

        // Propriétés calculées
        [NotMapped]
        public double TauxCouverture
        {
            get
            {
                if (Affectations == null || !Affectations.Any()) return 0;
                var servicesCouverts = Affectations.Select(a => a.ServiceId).Distinct().Count();
                return Math.Round((double)servicesCouverts / 4 * 100, 1); // 4 services au total
            }
        }

        [NotMapped]
        public int NombrePersonnelAssigne
        {
            get => Affectations?.Select(a => a.PersonnelId).Distinct().Count() ?? 0;
        }

        [NotMapped]
        public int HeuresTotal
        {
            get => Affectations?.Sum(a => (int)(a.HeureFin - a.HeureDebut).TotalHours) ?? 0;
        }

        [NotMapped]
        public string PeriodeAffichage => $"{DateDebut:dd MMM} - {DateFin:dd MMM yyyy}";

        [NotMapped]
        public string StatutLibelle
        {
            get
            {
                switch (Statut)
                {
                    case "Draft": return "Brouillon";
                    case "Pending": return "En attente d'approbation";
                    case "Approved": return "Approuvé";
                    case "Archived": return "Archivé";
                    case "Cancelled": return "Annulé";
                    default: return "Inconnu";
                }
            }
        }
    }
}