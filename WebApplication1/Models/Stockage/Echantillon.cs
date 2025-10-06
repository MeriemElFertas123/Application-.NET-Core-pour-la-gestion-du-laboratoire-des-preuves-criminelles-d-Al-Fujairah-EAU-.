using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models.Envoi;

namespace WebApplication1.Models.Stockage
{
    [Table("Echantillons")]
    public class Echantillon
    {
        [Key]
        public int Id { get; set; }

        // Identification
        [Required(ErrorMessage = "Le numéro d'échantillon est requis")]
        [StringLength(50, ErrorMessage = "Le numéro d'échantillon ne peut dépasser 50 caractères")]
        [Display(Name = "Numéro d'échantillon")]
        public string NumeroEchantillon { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Le numéro d'affaire ne peut dépasser 50 caractères")]
        [Display(Name = "Numéro d'affaire")]
        public string? NumeroAffaire { get; set; }

        // Relations principales
        [Display(Name = "Affaire")]
        public int? AffaireId { get; set; }

        public virtual Affaire? Affaire { get; set; }

        [Display(Name = "Stockage")]
        public int? StockageId { get; set; }

        public virtual Stockage? Stockage { get; set; }

        [Display(Name = "Créateur")]
        public string? CreateurId { get; set; }

        public virtual ApplicationUser? Createur { get; set; }

        [Display(Name = "Analyste")]
        public int? AnalysteId { get; set; }

        [ForeignKey("AnalysteId")]
        public virtual Analyste? Analyste { get; set; }

        [Display(Name = "Déjà envoyé")]
        public bool DejaEnvoye { get; set; } = false;

        [Display(Name = "Envoi complet")]
        public int? EnvoiCompletId { get; set; }

        [ForeignKey("EnvoiCompletId")]
        public virtual EnvoiComplet? EnvoiComplet { get; set; }

        // Caractéristiques de base
        [Display(Name = "Type d'échantillon")]
        public TypeEchantillon Type { get; set; }

        [Display(Name = "Statut")]
        public StatutEchantillon Statut { get; set; } = StatutEchantillon.EnAttente;

        [Display(Name = "Priorité")]
        public PrioriteEchantillon Priorite { get; set; } = PrioriteEchantillon.Normal;

        // Dates importantes
        [Display(Name = "Date de réception")]
        [DataType(DataType.DateTime)]
        public DateTime DateReception { get; set; } = DateTime.Now;

        [Display(Name = "Date début analyse")]
        [DataType(DataType.DateTime)]
        public DateTime? DateDebutAnalyse { get; set; }

        [Display(Name = "Date fin analyse")]
        [DataType(DataType.DateTime)]
        public DateTime? DateFinAnalyse { get; set; }

        [Display(Name = "Date limite")]
        [DataType(DataType.Date)]
        public DateTime? DateLimite { get; set; }

        [Display(Name = "Date archivage")]
        [DataType(DataType.DateTime)]
        public DateTime? DateArchivage { get; set; }

        // Description et localisation
        [StringLength(1000, ErrorMessage = "La description ne peut dépasser 1000 caractères")]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "Les conditions de stockage ne peuvent dépasser 500 caractères")]
        [Display(Name = "Conditions de stockage")]
        public string? ConditionsStockage { get; set; }

        [StringLength(200, ErrorMessage = "Le lieu de collecte ne peut dépasser 200 caractères")]
        [Display(Name = "Lieu de collecte")]
        public string? LieuCollecte { get; set; }

        [StringLength(100, ErrorMessage = "Le responsable de collecte ne peut dépasser 100 caractères")]
        [Display(Name = "Responsable de collecte")]
        public string? ResponsableCollecte { get; set; }

        // Gestion des fichiers
        [Display(Name = "Nom du fichier")]
        public string? NomFichier { get; set; }

        [Display(Name = "Chemin du fichier")]
        public string? CheminFichier { get; set; }

        [Display(Name = "Contenu du fichier")]
        public byte[]? ContenuFichier { get; set; }

        [Display(Name = "Type MIME")]
        public string? TypeMime { get; set; }

        [Display(Name = "Taille du fichier")]
        public long TailleFichier { get; set; }

        // Traçabilité
        [Display(Name = "QR Code")]
        public string? QRCode { get; set; }

        [Display(Name = "Notes supplémentaires")]
        [DataType(DataType.MultilineText)]
        public string? NotesSupplementaires { get; set; }

        // Analyses
        [StringLength(1000, ErrorMessage = "Les commentaires de l'analyste ne peuvent dépasser 1000 caractères")]
        [Display(Name = "Commentaires de l'analyste")]
        [DataType(DataType.MultilineText)]
        public string? CommentairesAnalyste { get; set; }

        // Collections de navigation
        public virtual ICollection<Analyse.Analyse> Analyses { get; set; } = new HashSet<Analyse.Analyse>();
        public virtual ICollection<RapportAnalyse> Rapports { get; set; } = new HashSet<RapportAnalyse>();

        // Propriétés calculées
        [NotMapped]
        [Display(Name = "Taille formatée")]
        public string TailleFichierFormatee => FormatTaille(TailleFichier);

        [NotMapped]
        [Display(Name = "A un fichier")]
        public bool AUnFichier => !string.IsNullOrEmpty(CheminFichier);

        [NotMapped]
        [Display(Name = "Durée de stockage")]
        public TimeSpan? DureeStockage => DateArchivage.HasValue ? DateArchivage.Value - DateReception : DateTime.Now - DateReception;

        public List<AttributionAnalyse>? AttributionAnalyses { get; internal set; }

        // Méthodes utilitaires
        private static string FormatTaille(long taille)
        {
            if (taille == 0) return "0 bytes";

            string[] suffixes = { "bytes", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double tailleDouble = taille;

            while (tailleDouble >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                tailleDouble /= 1024;
                suffixIndex++;
            }

            return $"{tailleDouble:F1} {suffixes[suffixIndex]}";
        }

        public bool PeutEtreModifie()
        {
            return Statut == StatutEchantillon.EnAttente ||
                   Statut == StatutEchantillon.Recu ||
                   Statut == StatutEchantillon.Stocke;
        }

        public bool PeutEtreSupprime()
        {
            return Statut == StatutEchantillon.EnAttente ||
                   Statut == StatutEchantillon.Archive;
        }

        public string GetStatutCssClass()
        {
            return Statut switch
            {
                StatutEchantillon.EnAttente => "badge-warning",
                StatutEchantillon.EnAnalyse => "badge-info",
                StatutEchantillon.AnalyseTerminee => "badge-success",
                StatutEchantillon.Archive => "badge-secondary",
                StatutEchantillon.Stocke => "badge-primary",
                StatutEchantillon.EnValidation => "badge-warning",
                StatutEchantillon.Valide => "badge-success",
                StatutEchantillon.Recu => "badge-info",
                StatutEchantillon.Verifie => "badge-success",
                StatutEchantillon.Envoye => "badge-primary",
                _ => "badge-light"
            };
        }

        public string GetPrioriteCssClass()
        {
            return Priorite switch
            {
                PrioriteEchantillon.Critique => "text-danger font-weight-bold",
                PrioriteEchantillon.Urgent => "text-warning font-weight-bold",
                PrioriteEchantillon.Haute => "text-info",
                PrioriteEchantillon.Normal => "text-muted",
                PrioriteEchantillon.Basse => "text-secondary",
                _ => "text-muted"
            };
        }
    }

    public enum TypeEchantillon
    {
        [Display(Name = "Sang")]
        Sang,

        [Display(Name = "Salive")]
        Salive,

        [Display(Name = "Urine")]
        Urine,

        [Display(Name = "Tissu")]
        Tissu,

        [Display(Name = "Cheveux")]
        Cheveux,

        [Display(Name = "Liquide")]
        Liquide,

        [Display(Name = "Solide")]
        Solide,

        [Display(Name = "Autre")]
        Autre
    }

    public enum StatutEchantillon
    {
        [Display(Name = "En attente")]
        EnAttente,

        [Display(Name = "En analyse")]
        EnAnalyse,

        [Display(Name = "Analyse terminée")]
        AnalyseTerminee,

        [Display(Name = "Archivé")]
        Archive,

        [Display(Name = "Stocké")]
        Stocke,

        [Display(Name = "En validation")]
        EnValidation,

        [Display(Name = "Validé")]
        Valide,

        [Display(Name = "Reçu")]
        Recu,

        [Display(Name = "Vérifié")]
        Verifie,

        [Display(Name = "Envoyé")]
        Envoye
    }

    public enum PrioriteEchantillon
    {
        [Display(Name = "Basse")]
        Basse,

        [Display(Name = "Normale")]
        Normal,

        [Display(Name = "Haute")]
        Haute,

        [Display(Name = "Urgent")]
        Urgent,

        [Display(Name = "Critique")]
        Critique
    }
}