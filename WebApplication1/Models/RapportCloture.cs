using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using WebApplication1.ViewModels;

namespace WebApplication1.Models
{
    public class RapportCloture
    {
        private string statutResolution;

        public int Id { get; set; }
        public int AffaireId { get; set; }

        [NotMapped]
        public string StatutResolution;
        public DateTime DateCloture { get; set; }
        public string ResumeActions { get; set; }
        public string ResultatsObtenus { get; set; }
        public string Recommandations { get; set; }

        public string NotesComplementaires { get; set; }
        public DateTime DateCreation { get; set; }
        public int AuteurId { get; set; }

        // Navigation
        public virtual Affaire Affaire { get; set; }
    }
}