using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Models
{
    public class RapportAnalyse
    {
        [Key]
        public int idRapport { get; set; }

        [Required]
        public string conclusion { get; set; }

        public DateTime dateRapport { get; set; }

        // ===== Relation avec Echantillon =====
        public int idPreuve { get; set; }

        [ForeignKey("idPreuve")]

        public virtual Echantillon preuve { get; set; }

        // ===== Relation avec Analyste =====
        public int idAnalyste { get; set; }

        [ForeignKey("idAnalyste")]
        public virtual Analyste analyste { get; set; }

        // ===== Relation avec Analyse =====
        public int? AnalyseId { get; set; }

        [ForeignKey("AnalyseId")]

        public virtual Analyse.Analyse Analyse { get; set; }
    }

}
