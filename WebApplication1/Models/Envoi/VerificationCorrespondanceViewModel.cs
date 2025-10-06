// Models/Envoi/VerificationCorrespondanceViewModel.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace WebApplication1.Models.Envoi
{
    public class VerificationCorrespondanceViewModel
    {
        public int EnvoiEchantillonId { get; set; }
        public EnvoiEchantillonReceptionDto EchantillonInfo { get; set; }
        public List<CritereVerificationCorrespondance> CriteresVerification { get; set; }
        public string ObservationsGenerales { get; set; }
    }

    public class CritereVerificationCorrespondance
    {
        [Key]
        public int id {  get; set; }
        [Required]
        [StringLength(100)]
        public string Nom { get; set; }

        [StringLength(50)]
        public string Icone { get; set; }

        [Required]
        [StringLength(200)]
        public string ValeurAttendue { get; set; }

        [StringLength(200)]
        public string ValeurConstatee { get; set; }

        public bool EstVerifie { get; set; }

        public bool EstConforme { get; set; }

        [StringLength(1000)]
        public string Observations { get; set; }

        public bool Obligatoire { get; set; } = true;

        [StringLength(50)]
        public string TypeVerification { get; set; } // "numeric", "select", "radio", "text"

        public List<string> OptionsDisponibles { get; set; } = new List<string>();

        // Pour les vérifications numériques
        public double? ToleranceMin { get; set; }
        public double? ToleranceMax { get; set; }

        // Métadonnées pour l'affichage
        public string CssClass => EstVerifie ? (EstConforme ? "conforme" : "non-conforme") : "pending";
        public string IconeStatut => EstVerifie ? (EstConforme ? "fa-check" : "fa-times") : "fa-clock";
        public string CouleurStatut => EstVerifie ? (EstConforme ? "success" : "danger") : "warning";
    }

   public class TraiterVerificationViewModel
    {
        public int EnvoiEchantillonId { get; set; }
        public string ObservationsGenerales { get; set; }
        public List<CritereVerificationDto> CriteresVerification { get; set; }

        public TraiterVerificationViewModel()
        {
            CriteresVerification = new List<CritereVerificationDto>();
        }
    }

    public class CritereVerificationDto
    {
        public string Nom { get; set; }
        public string ValeurConstatee { get; set; }
        public string Observations { get; set; }
        public bool EstConforme { get; set; }
    }

}