using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class HistoriqueAction
    {
        public int Id { get; set; }
        public int AffaireId { get; set; }
        public string TypeAction { get; set; } // "Clôture", "Archivage", "Modification", etc.
        public string Description { get; set; }
        public DateTime DateAction { get; set; }
        public int UtilisateurId { get; set; }
        public string DonneesAvant { get; set; } // JSON des données avant modification
        public string DonneesApres { get; set; } // JSON des données après modification

        // Navigation
        public virtual Affaire Affaire { get; set; }
    }
}