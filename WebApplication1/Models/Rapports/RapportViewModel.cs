using System;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models.Analyse;

namespace WebApplication1.Models.ViewModels
{
    public class RapportViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Nom du rapport")]
        public string Nom { get; set; }

        [Display(Name = "Type d'analyse")]
        public string TypeAnalyse { get; set; }

        [Display(Name = "Date d'analyse")]
        [DataType(DataType.Date)]
        public DateTime DateAnalyse { get; set; }

        [Display(Name = "Statut")]
        public StatutAnalyse Statut { get; set; }

        [Display(Name = "Validé")]
        public bool EstValide { get; set; }

        [Display(Name = "Échantillon")]
        public string EchantillonNom { get; set; }

        // Pour les filtres
        public string FiltreStatut { get; set; }
        public string FiltreType { get; set; }
        public string FiltrePeriode { get; set; }
    }
}