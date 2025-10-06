using System;
using System.Collections.Generic;

namespace WebApplication1.Controllers
{
    internal class DetailEnvoiViewModel
    {
        public int Id { get; set; }
        public string NumeroBon { get; set; }
        public string NomLaboratoire { get; set; }
        public string TypeAnalyse { get; set; }
        public string NomAffaire { get; set; }
        public DateTime DateEnvoiPrevue { get; set; }
        public DateTime DateCreation { get; set; }
        public string StatutEnvoi { get; set; }
        public string ObservationsEnvoi { get; set; }
        public List<EchantillonEnvoyeViewModel> EchantillonsEnvoyes { get; set; }
        public string CreePar { get; internal set; }
    }
}