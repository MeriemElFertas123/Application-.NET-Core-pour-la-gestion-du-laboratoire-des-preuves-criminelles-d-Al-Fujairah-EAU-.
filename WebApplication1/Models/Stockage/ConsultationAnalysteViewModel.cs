using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Stockage
{
    public class ConsultationAnalysteViewModel
    {
        public string NomAnalyste { get; set; }
        public List<EchantillonAConsulter> Echantillons { get; set; }

        public ConsultationAnalysteViewModel()
        {
            Echantillons = new List<EchantillonAConsulter>();
        }
    }

    public class EchantillonAConsulter
    {
        public int Id { get; set; }
        public string NumeroEchantillon { get; set; }
        public string Type { get; set; }
        public string Statut { get; set; }
        public string Priorite { get; set; }
        public DateTime DateReception { get; set; }
        public string Zone { get; set; }
        public string Emplacement { get; set; }
    }

}