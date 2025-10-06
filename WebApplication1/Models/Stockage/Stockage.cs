using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Envoi;

namespace WebApplication1.Models.Stockage
{
  
    public class Stockage
    {
        [Key]
        public int Id { get; set; }

        public int EchantillonId { get; set; }
        public virtual Echantillon Echantillon { get; set; }

        [Required, StringLength(10)]
        public string Zone { get; set; }

        [Required, StringLength(10)]
        public string Emplacement { get; set; }

        public DateTime DateStockage { get; set; }

        public string TechnicienId { get; set; }

        public StatutStockage Statut { get; set; }

        [NotMapped]
        public string EmplacementComplet => $"{Zone}-{Emplacement}";
    }

    public enum StatutStockage { Reserve, Stocke, PretPourAnalyse, EnAnalyse, Retourne, Archive }

    // ---------------------- ZoneStockage ----------------------
    public class ZoneStockage
    {
        [Key, StringLength(10)]
        public string Code { get; set; }

        [Required, StringLength(100)]
        public string Nom { get; set; }

        public int Temperature { get; set; }
        public int Capacite { get; set; }
        public int Occupe { get; set; }

        [NotMapped]
        public int Libre => Capacite - Occupe;

        [NotMapped]
        public double TauxOccupation => Capacite > 0 ? Math.Round((double)Occupe / Capacite * 100, 1) : 0;

        public virtual ICollection<Emplacement> Emplacements { get; set; } = new HashSet<Emplacement>();
    }

    // ---------------------- Emplacement ----------------------
    public class Emplacement
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(10)]
        public string Zone { get; set; }

        [Required, StringLength(10)]
        public string Numero { get; set; }

        public bool EstOccupe { get; set; }

        public int? EchantillonId { get; set; }
        public DateTime? DateOccupation { get; set; }

        [NotMapped]
        public string EmplacementComplet => $"{Zone}-{Numero}";

        public virtual ZoneStockage ZoneStockage { get; set; }
        public virtual Echantillon Echantillon { get; set; }
    }

    // ---------------------- ViewModels Stockage ----------------------
    public class StockageViewModel
    {
        public List<EnvoiEchantillonReceptionDto> EchantillonsEnAttente { get; set; } = new List<EnvoiEchantillonReceptionDto>();
        public List<ZoneStockage> ZonesStockage { get; set; } = new List<ZoneStockage>();
        public StatistiquesStockage Statistiques { get; set; } = new StatistiquesStockage();
    }

    public class StockerEchantillonViewModel
    {
        public Echantillon Echantillon { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner une zone")]
        [Display(Name = "Zone de stockage")]
        public string ZoneSelectionnee { get; set; }

        [Required(ErrorMessage = "Veuillez sélectionner un emplacement")]
        [Display(Name = "Emplacement")]
        public string EmplacementSelectionne { get; set; }

        [Display(Name = "Conditions spéciales")]
        [StringLength(500)]
        public string ConditionsSpeciales { get; set; }

        [Display(Name = "Date limite de stockage")]
        [DataType(DataType.Date)]
        public DateTime? DateLimite { get; set; }

        [Display(Name = "Stockage prioritaire")]
        public bool EstPrioritaire { get; set; }

        public List<ZoneStockage> ZonesDisponibles { get; set; } = new List<ZoneStockage>();
        public List<Emplacement> EmplacementsLibres { get; set; } = new List<Emplacement>();
    }

    public class AttribuerEmplacementViewModel
    {
        public List<Echantillon> EchantillonsEnAttente { get; set; } = new List<Echantillon>();
        public PlanStockageData PlanStockage { get; set; } = new PlanStockageData();
        public List<AttributionEmplacement> Attributions { get; set; } = new List<AttributionEmplacement>();
    }

    public class AttributionEmplacement
    {
        [Display(Name = "Sélectionner")]
        public bool IsSelected { get; set; }

        public int EchantillonId { get; set; }
        public string NumeroEchantillon { get; set; }

        [Required(ErrorMessage = "Zone requise")]
        public string Zone { get; set; }

        [Required(ErrorMessage = "Emplacement requis")]
        public string Emplacement { get; set; }

        public string TypeEchantillon { get; set; }
        public string Priorite { get; set; }
    }

    public class PlanStockageViewModel
    {
        public List<ZoneStockage> Zones { get; set; } = new List<ZoneStockage>();
        public List<Emplacement> Emplacements { get; set; } = new List<Emplacement>();
        public FiltresStockage Filtres { get; set; } = new FiltresStockage();
        public StatistiquesStockage Statistiques { get; set; } = new StatistiquesStockage();
    }

    public class RechercheStockageViewModel
    {
        [Display(Name = "Terme de recherche")]
        public string Terme { get; set; }
        [Display(Name = "Zone")]
        public string ZoneSelectionnee { get; set; }
        [Display(Name = "Statut")]
        public string StatutSelectionne { get; set; }
        [Display(Name = "Type")]
        public string TypeSelectionne { get; set; }

        public List<Echantillon> Resultats { get; set; } = new List<Echantillon>();
        public List<ZoneStockage> Zones { get; set; } = new List<ZoneStockage>();
        public List<string> Statuts { get; set; } = new List<string>();
        public List<string> Types { get; set; } = new List<string>();
    }

    public class FiltresStockage
    {
        public string Zone { get; set; }
        public string Statut { get; set; }
        public string Type { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
    }

    public class StatistiquesStockage
    {
        public int TotalCapacite { get; set; }
        public int TotalOccupe { get; set; }
        public int TotalLibre => TotalCapacite - TotalOccupe;
        public int EchantillonsEnAttente { get; set; }
        public double TauxOccupation { get; set; }
    }

    public class PlanStockageData
    {
        public Dictionary<string, List<Emplacement>> EmplacementsParZone { get; set; } = new Dictionary<string, List<Emplacement>>();
        public List<ZoneStockage> Zones { get; set; } = new List<ZoneStockage>();
    }
}
