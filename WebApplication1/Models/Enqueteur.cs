using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using WebApplication1.Models;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Models
{
    // Enqueteur.cs
    public class Enqueteur
    {
        [Key]
        public int idEnqueteur { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [Display(Name = "Nom")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        public string nom { get; set; }

        [Required(ErrorMessage = "Le prénom est requis")]
        [Display(Name = "Prénom")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        public string prenom { get; set; }

        [Required(ErrorMessage = "Le grade est requis")]
        [Display(Name = "Grade")]
        [StringLength(100, ErrorMessage = "Le grade ne peut pas dépasser 100 caractères")]
        public string grade { get; set; }

        [Display(Name = "Service")]
        [StringLength(100, ErrorMessage = "Le service ne peut pas dépasser 100 caractères")]
        public string service { get; set; }

        [ForeignKey("User")]
        [StringLength(450)]
        public string? UserId { get; set; }

        public virtual ApplicationUser? User { get; set; }


        // Collection de preuves (ne sera jamais validée)
        public virtual ICollection<Echantillon> preuves { get; set; } = new List<Echantillon>();

        // Propriété calculée pour l'affichage
        [Display(Name = "Nom complet")]
        public string NomComplet => $"{prenom} {nom}";
    }
}