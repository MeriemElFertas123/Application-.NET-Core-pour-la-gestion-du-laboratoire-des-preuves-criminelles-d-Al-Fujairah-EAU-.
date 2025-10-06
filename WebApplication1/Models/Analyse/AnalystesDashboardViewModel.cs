using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebApplication1.Models.Stockage;
using WebApplication1.Models.Analyse;

namespace WebApplication1.Models.Analyse
{
    public class AnalystesDashboardViewModel
    {
        public List<Echantillon> EchantillonsAssignes { get; set; }
        public List<Echantillon> AnalysesEnCours { get; set; }
        public List<Echantillon> AnalysesTerminees { get; set; }
        public int NombreAssignes { get; set; }
        public int NombreEnCours { get; set; }
        public int NombreTerminees { get; set; }
        public int NombreAValider { get; set; }
    }

    public class SaisieResultatsViewModel
    {
        public int EchantillonId { get; set; }
        public Echantillon Echantillon { get; set; }

        [Required]
        [Display(Name = "Type d'analyse")]
        public string TypeAnalyse { get; set; }

        [Required]
        [Display(Name = "Méthode utilisée")]
        public string Methode { get; set; }

        [Required]
        [Display(Name = "Résultats")]
        [StringLength(2000)]
        public string Resultats { get; set; }

        [Required]
        [Display(Name = "Conclusion")]
        [StringLength(1000)]
        public string Conclusion { get; set; }

        [Display(Name = "Observations complémentaires")]
        [StringLength(1000)]
        public string Observations { get; set; }
    }

    public class RapportTechniqueViewModel
    {
        public Echantillon Echantillon { get; set; }
        public List<Analyse> Analyses { get; set; }
        public Analyste Analyste { get; set; }
        public DateTime DateGeneration { get; set; } = DateTime.Now;
    }
   
}
