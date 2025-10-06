using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Envoi
{
    public class PreparationEnvoiViewModel
    {
        [Required]
        public int AffaireId { get; set; }

        [Required]
        public string TypeAnalyseDemandee { get; set; }

        [Required]
        public DateTime DateEnvoiPrevue { get; set; }

        public string ObservationsEnvoi { get; set; }

        public List<int> EchantillonsSelectionnes { get; set; } = new List<int>();
        public string EchantillonsDetailsJson { get; set; }
        public string QRCodesJson { get; set; }

        public List<EchantillonDisponibleModel> EchantillonsDisponibles { get; set; }
        public string Observations { get; internal set; }
    }
}