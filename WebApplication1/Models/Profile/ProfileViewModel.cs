using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Account
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100, ErrorMessage = "Le nom ne peut pas dépasser 100 caractères")]
        public string nom { get; set; }

        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(100, ErrorMessage = "Le prénom ne peut pas dépasser 100 caractères")]
        public string prenom { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(256, ErrorMessage = "L'email ne peut pas dépasser 256 caractères")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Format de téléphone invalide")]
        public string PhoneNumber { get; set; }

        // Champs en lecture seule
        public string Id { get; set; }
        public string UserName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime? LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }

        // Propriétés calculées
        public string NomComplet => $"{prenom} {nom}";

        public string InitialesUtilisateur
        {
            get
            {
                var initiales = "";
                if (!string.IsNullOrEmpty(prenom))
                    initiales += prenom[0];
                if (!string.IsNullOrEmpty(nom))
                    initiales += nom[0];
                return initiales.ToUpper();
            }
        }
    }
}