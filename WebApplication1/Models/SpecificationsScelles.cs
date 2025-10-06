using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class SpecificationsScelles
    {
        [Key]
        public int ScelleId { get; set; }

        [Required]
        public float Poids { get; set; }

        [Required]
        public float Longueur { get; set; }

        [Required]
        public float Largeur { get; set; }

        [Required]
        public float Hauteur { get; set; }

        [Required]
        [StringLength(50)]
        public string Couleur { get; set; }

        [Required]
        public int NombrePieces { get; set; }

        [StringLength(255)]
        public string Description { get; set; }
    }
}