using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class TypesAnalyses
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } // Exemple : Mabda2i, Kimya2i, Ta2kidi

        [StringLength(500)]
        public string Description { get; set; }

        [StringLength(50)]
        public string NiveauAcces { get; set; } // Exemple : Bas, Moyen, Élevé
    }
}