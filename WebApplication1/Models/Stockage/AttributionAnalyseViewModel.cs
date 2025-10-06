// Models/Stockage/AttributionAnalyseViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Models.Stockage
{
    public class AttributionAnalyseViewModel
    {
        [Display(Name = "Échantillons disponibles")]
        public List<EchantillonPourAttribution> EchantillonsDisponibles { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un analyste.")]
        [Display(Name = "Sélectionner l'analyste")]
        public int AnalysteId { get; set; }

        [Required(ErrorMessage = "Veuillez définir le type d'analyse.")]
        [Display(Name = "Type d'analyse requis")]
        public string TypeAnalyseRequis { get; set; }

        [Required(ErrorMessage = "Veuillez définir la priorité.")]
        [Display(Name = "Définir la priorité")]
        public string PrioriteAnalyse { get; set; }

        [Display(Name = "Ajouter instructions spéciales")]
        public string InstructionsSpeciales { get; set; }

        [Display(Name = "Notifier l'analyste")]
        public bool NotifierAnalyste { get; set; } = true;

        public List<SelectListItem> AnalystesDisponibles { get; set; }
        public List<SelectListItem> TypesAnalyseDisponibles { get; set; }
        public List<SelectListItem> NiveauxPriorite { get; set; }

        public AttributionAnalyseViewModel()
        {
            EchantillonsDisponibles = new List<EchantillonPourAttribution>();
            AnalystesDisponibles = new List<SelectListItem>();
            TypesAnalyseDisponibles = new List<SelectListItem>();
            NiveauxPriorite = new List<SelectListItem>();
        }
    }

    public class EchantillonPourAttribution
    {
        public int Id { get; set; }
        public bool EstSelectionne { get; set; }
        public string NumeroEchantillon { get; set; }
        public string NumeroAffaire { get; set; }
        public string TypeEchantillon { get; set; }
        public string Zone { get; set; }
        public string Emplacement { get; set; }
        public string Statut { get; set; }
        public string Priorite { get; set; }
        public DateTime DateReception { get; set; }
    }
}

// Models/Enums.cs - Simples énumérations
namespace WebApplication1.Models
{
    public enum TypeAnalyse
    {
        AnalyseChimique,
        AnalyseBiologique,
        AnalyseToxicologique,
        AnalyseADN,
        AnalyseBalistique,
        AnalyseEmpreintes,
        AnalyseDocumentaire
    }

    public enum PrioriteAnalyse
    {
        Urgente,
        Elevee,
        Normale,
        Faible
    }
}

    public class AttributionAnalyse
    {
        [Key]
        public int Id { get; set; }

        public int EchantillonId { get; set; }
        [ForeignKey("EchantillonId")]
        public virtual Echantillon Echantillon { get; set; }

        public int AnalysteId { get; set; }
        [ForeignKey("AnalysteId")]
        public virtual Analyste Analyste { get; set; }

        public string TypeAnalyseRequis { get; set; }
        public string PrioriteAnalyse { get; set; }
        public string InstructionsSpeciales { get; set; }
        public DateTime DateAttribution { get; set; }
        public string AttributePar { get; set; }
        public bool NotificationEnvoyee { get; set; }
    }

