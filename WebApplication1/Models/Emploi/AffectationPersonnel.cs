using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Emploi
{
    public class AffectationPersonnel
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Emploi du temps")]
        public int EmploiTempsId { get; set; }

        [Required]
        [Display(Name = "Personnel")]
        public int PersonnelId { get; set; }

        [Required]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Jour")]
        public string Jour { get; set; } // Monday, Tuesday, Wednesday, Thursday, Friday

        [Required]
        [StringLength(20)]
        [Display(Name = "Équipe")]
        public string Equipe { get; set; } // Morning, Evening

        [Required]
        [Display(Name = "Heure de début")]
        public TimeSpan HeureDebut { get; set; }

        [Required]
        [Display(Name = "Heure de fin")]
        public TimeSpan HeureFin { get; set; }

        [Display(Name = "Date de création")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Relations
        [ForeignKey("EmploiTempsId")]
        public virtual EmploiTemps EmploiTemps { get; set; }

        [ForeignKey("PersonnelId")]
        public virtual Personnel Personnel { get; set; }

        [ForeignKey("ServiceId")]
        public virtual Service Service { get; set; }

        // Propriétés calculées
        [NotMapped]
        public int DureeEnHeures => (int)(HeureFin - HeureDebut).TotalHours;

        [NotMapped]
        public string JourLibelle
        {
            get
            {
                switch (Jour)
                {
                    case "Monday": return "Lundi";
                    case "Tuesday": return "Mardi";
                    case "Wednesday": return "Mercredi";
                    case "Thursday": return "Jeudi";
                    case "Friday": return "Vendredi";
                    default: return "Inconnu";
                }
            }
        }

        [NotMapped]
        public string EquipeLibelle
        {
            get
            {
                switch (Equipe)
                {
                    case "Morning": return "Matin";
                    case "Evening": return "Soir";
                    default: return "Inconnu";
                }
            }
        }
    }
}