using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Emploi
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nom du service")]
        public string Nom { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Capacité maximum")]
        public int CapaciteMaximum { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Code couleur")]
        public string CodeCouleur { get; set; }

        [Display(Name = "Actif")]
        public bool Actif { get; set; } = true;

        // Relations
        public virtual ICollection<Personnel> Personnels { get; set; } = new List<Personnel>();
        public virtual ICollection<AffectationPersonnel> Affectations { get; set; } = new List<AffectationPersonnel>();

    }
}