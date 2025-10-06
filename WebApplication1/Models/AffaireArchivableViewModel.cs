using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class AffaireArchivableViewModel
    {
        public int Id { get; set; }
        public string Titre { get; set; }
        public DateTime DateFermeture { get; set; }
        public string Statut { get; set; }
        public string Priorite { get; set; }
        public bool EstSelectionne { get; set; }
        public string EnqueteurResponsable { get; set; }
        public int NombrePreuves { get; set; }
        public long TailleEstimee { get; set; } // En bytes
    }
    // Enum pour les motifs d'archivage
    public enum MotifArchivage
    {
        [Display(Name = "Archivage automatique - Délai écoulé")]
        ArchivageAutomatique,

        [Display(Name = "Archivage manuel - Affaire terminée")]
        ArchivageManuel,

        [Display(Name = "Transfert vers archives centrales")]
        TransfertArchives,

        [Display(Name = "Libération d'espace de stockage")]
        LiberationEspace,

        [Display(Name = "Conformité réglementaire")]
        ConformiteReglementaire
    }

}
