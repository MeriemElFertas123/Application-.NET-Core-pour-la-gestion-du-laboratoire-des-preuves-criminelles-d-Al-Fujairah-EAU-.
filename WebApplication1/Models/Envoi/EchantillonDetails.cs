using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Envoi
{
    public class EchantillonDetails
    {
        public decimal Poids { get; set; }
        public string ConditionsStockage { get; set; }
        public string Couleur { get; set; }
        public string Emballage { get; set; }
        public string Emplacement { get; set; }
        public string Observations { get; set; }
    }
}