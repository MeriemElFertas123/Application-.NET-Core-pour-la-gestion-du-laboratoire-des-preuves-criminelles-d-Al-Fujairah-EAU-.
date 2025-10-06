using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    // ViewModel pour l'archivage
    public class ArchiverAffaireViewModel
    {
        public List<AffaireArchivableViewModel> AffairesArchivables { get; set; } = new List<AffaireArchivableViewModel>();

        [Required(ErrorMessage = "Le motif d'archivage est obligatoire")]
        [Display(Name = "Motif d'archivage")]
        public string MotifArchivage { get; set; }

        [Display(Name = "Notes sur l'archivage")]
        public string NotesArchivage { get; set; }

        // Filtres
        public DateTime? DateDebutFiltre { get; set; }
        public DateTime? DateFinFiltre { get; set; }
        public string PrioriteFiltre { get; set; }
        public string StatutFiltre { get; set; }

        // Statistiques
        public int NombreAffairesArchivables { get; set; }
        public int NombreAffairesSelectionnees { get; set; }
    }
}