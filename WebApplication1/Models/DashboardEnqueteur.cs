// Models/DashboardEnqueteur.cs
using System.Collections.Generic;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Models
{
    public class DashboardEnqueteur
    {
        // Totaux
        public int TotalAffaires { get; set; }
        public int TotalEchantillons { get; set; }
        public int TotalEnvoisLabo { get; set; }

        // Répartition des affaires
        public Dictionary<StatutAffaire, int> AffairesParStatut { get; set; }
        public Dictionary<Priorite, int> AffairesParPriorite { get; set; }

        // Répartition des échantillons
        public Dictionary<StatutEchantillon, int> EchantillonsParStatut { get; set; }
        public Dictionary<TypeEchantillon, int> EchantillonsParType { get; set; }
        public Dictionary<PrioriteEchantillon, int> EchantillonsParPriorite { get; set; }

        // Répartition des analyses
        public int AnalysesTerminees { get; set; }
        public int AnalysesEnCours { get; set; }

        // Listes récentes
        public List<Affaire> AffairesRecentes { get; set; }
        public List<Echantillon> EchantillonsRecents { get; set; }
    }
}