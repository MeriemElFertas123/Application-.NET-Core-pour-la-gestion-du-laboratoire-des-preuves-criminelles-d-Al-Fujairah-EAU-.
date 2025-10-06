using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Models.Analyse
{
    public class Analyse
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        public bool EstValide { get; set; }

        [ForeignKey("Echantillon")]
        public int EchantillonId { get; set; }
        public virtual Echantillon Echantillon { get; set; }

        public int AnalysteId { get; set; }
        public DateTime DateAnalyse { get; set; }

        [StringLength(100)]
        public string TypeAnalyse { get; set; } = string.Empty;

        [StringLength(200)]
        public string Methode { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Conclusion { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Resultats { get; set; } = string.Empty;

        [StringLength(255)]
        public string NomFichier { get; set; } = string.Empty;

        public byte[] FichierContenu { get; set; } = Array.Empty<byte>(); // Initialiser avec un tableau vide

        [StringLength(50)]
        public string FichierContentType { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Observations { get; set; } = string.Empty;

        public StatutAnalyse Statut { get; set; }
    }

    public enum StatutAnalyse
    {
        EnCours,
        Terminee,
        Validee,
        Reportee
    }
}