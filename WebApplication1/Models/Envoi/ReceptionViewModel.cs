// ViewModels/Envoi/ReceptionViewModel.cs
using System;
using System.Collections.Generic;

namespace WebApplication1.Models.Envoi
{
    public class ReceptionViewModel
    {
        public List<EnvoiEchantillonReceptionDto> EchantillonsRecus { get; set; } = new List<EnvoiEchantillonReceptionDto>();
        public StatistiquesReception Statistiques { get; set; } = new StatistiquesReception();
        public FiltresReception Filtres { get; set; } = new FiltresReception();
    }

    public class EnvoiEchantillonReceptionDto
    {
        public int Id { get; set; }
        public int EnvoiId { get; set; }
        public int EchantillonId { get; set; }
        public string CodeQR { get; set; }
        public string NumeroAffaire { get; set; }
        public string NomEnqueteur { get; set; }
        public string DescriptionEchantillon { get; set; }
        public string TypeAnalyse { get; set; }
        public decimal? Poids { get; set; }
        public string Couleur { get; set; }
        public string ConditionsStockage { get; set; }
        public string Emballage { get; set; }
        public string StatutEchantillon { get; set; }
        public string Priorite { get; set; }
        public DateTime? DateEnvoi { get; set; }
        public DateTime? DateReception { get; set; }
        public DateTime? DateVerification { get; set; }
        public string VerifiePar { get; set; }
        public string Observations { get; set; }
        public bool PeutEtreVerifie => StatutEchantillon == "Reçu";
        public bool EstVerifie => StatutEchantillon == "Vérifié" || StatutEchantillon == "Conforme" || StatutEchantillon == "Accepté";
        public bool EstRefuse => StatutEchantillon == "Non-conforme" || StatutEchantillon == "Refusé";
    }

    public class StatistiquesReception
    {
        public int EnAttente { get; set; }
        public int Verifies { get; set; }
        public int Refuses { get; set; }
        public int Stockes { get; set; }
    }

    public class FiltresReception
    {
        public string Recherche { get; set; }
        public string Statut { get; set; }
        public string Priorite { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }
}