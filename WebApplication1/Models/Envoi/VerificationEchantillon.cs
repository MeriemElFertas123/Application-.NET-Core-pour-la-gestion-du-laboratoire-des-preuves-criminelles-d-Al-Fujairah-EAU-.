// Models/Envoi/VerificationEchantillon.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models.Envoi
{
    public class VerificationEchantillon
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EnvoiEchantillonId { get; set; }

        [Required]
        [StringLength(50)]
        public string Critere { get; set; } // "Poids", "Couleur", "Quantité", "Texture", "Emballage", etc.

        [Required]
        [StringLength(200)]
        public string ValeurAttendue { get; set; }

        [Required]
        [StringLength(200)]
        public string ValeurConstatee { get; set; }

        [Required]
        public bool EstConforme { get; set; }

        [StringLength(500)]
        public string Observations { get; set; }

        public DateTime DateVerification { get; set; }

        [Required]
        [StringLength(100)]
        public string VerifiePar { get; set; }

        // Relations
        [ForeignKey("EnvoiEchantillonId")]
        public virtual EnvoiEchantillon EnvoiEchantillon { get; set; }
    }
}