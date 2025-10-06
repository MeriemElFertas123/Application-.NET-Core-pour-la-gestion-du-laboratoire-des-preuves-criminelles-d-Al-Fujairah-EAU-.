using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    // Modèle pour les archives
    public class AffaireArchive
    {
        public int Id { get; set; }
        public int AffaireId { get; set; }
        public DateTime DateArchivage { get; set; }
        public string MotifArchivage { get; set; }
        public int UtilisateurArchivage { get; set; }
        public string EmplacementStockage { get; set; }
        public string DonneesSerialisees { get; set; } // JSON de l'affaire complète
        public DateTime DateCreation { get; set; } = DateTime.Now;

        // Métadonnées pour la recherche
        public string TitreOriginal { get; set; }
        public DateTime DateOuvertureOriginale { get; set; }
        public DateTime DateFermetureOriginale { get; set; }
        public string StatutOriginal { get; set; }
        public string PrioriteOriginale { get; set; }
    }
}