using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Models
{
    public class Affaire
    {
        // Propriétés de base
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Le titre est obligatoire")]
        [StringLength(200, ErrorMessage = "Le titre ne peut pas dépasser 200 caractères")]
        public string Titre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La description est obligatoire")]
        [StringLength(2000, ErrorMessage = "La description ne peut pas dépasser 2000 caractères")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "datetime2")]
        public DateTime DateOuverture { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime? DateFermeture { get; set; } // Nullable si non clôturée

        // Statut avec enum dédié
        [Required]
        public StatutAffaire Statut { get; set; } = StatutAffaire.Ouverte;

        // Liens métiers
        [Required(ErrorMessage = "Un enquêteur responsable doit être assigné")]
        public int IdEnqueteurResponsable { get; set; }

        // Stockage JSON pour les motifs récurrents
        [Column(TypeName = "nvarchar(max)")]
        public string? MotifsRecurrentsJson { get; set; }

        // Propriété de navigation pour les motifs récurrents
        [NotMapped]
        public List<string> MotifsRecurrents
        {
            get
            {
                if (string.IsNullOrEmpty(MotifsRecurrentsJson))
                    return new List<string>();

                try
                {
                    return JsonSerializer.Deserialize<List<string>>(MotifsRecurrentsJson) ?? new List<string>();
                }
                catch
                {
                    return new List<string>();
                }
            }
            set
            {
                MotifsRecurrentsJson = value?.Count > 0
                    ? JsonSerializer.Serialize(value)
                    : null;
            }
        }

        // Métadonnées
        [Required]
        public Priorite Priorite { get; set; } = Priorite.Moyenne;

        [StringLength(500, ErrorMessage = "Le lieu ne peut pas dépasser 500 caractères")]
        public string? Lieu { get; set; }

        // Navigation properties
        public virtual ICollection<Echantillon> Echantillons { get; set; } = new List<Echantillon>();

        [StringLength(450)] // Taille standard pour les UserIds ASP.NET Core Identity
        public string? UserId { get; set; }

        [StringLength(50)]
        [Display(Name = "Numéro d'affaire")]
        public string? NumeroAffaire { get; set; }

        [StringLength(200)]
        [Display(Name = "Nom de l'enquêteur")]
        public string? NomEnqueteur { get; set; }

        // Propriétés d'audit
        [Column(TypeName = "datetime2")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime DateModification { get; set; } = DateTime.Now;

        [StringLength(450)]
        public string? CreeParUserId { get; set; }

        [StringLength(450)]
        public string? ModifieParUserId { get; set; }

        // Méthodes utiles
        public void AjouterMotifRecurrent(string motif)
        {
            if (string.IsNullOrWhiteSpace(motif))
                return;

            var motifs = MotifsRecurrents;
            if (!motifs.Contains(motif, StringComparer.OrdinalIgnoreCase))
            {
                motifs.Add(motif.Trim());
                MotifsRecurrents = motifs;
            }
        }

        public void SupprimerMotifRecurrent(string motif)
        {
            if (string.IsNullOrWhiteSpace(motif))
                return;

            var motifs = MotifsRecurrents;
            motifs.RemoveAll(m => string.Equals(m, motif.Trim(), StringComparison.OrdinalIgnoreCase));
            MotifsRecurrents = motifs;
        }

        public bool EstLiee(Affaire autreAffaire)
        {
            if (autreAffaire == null)
                return false;

            // Comparer les motifs récurrents de manière insensible à la casse
            return MotifsRecurrents
                .Intersect(autreAffaire.MotifsRecurrents, StringComparer.OrdinalIgnoreCase)
                .Any();
        }

        public bool EstOuverte()
        {
            return Statut == StatutAffaire.Ouverte || Statut == StatutAffaire.EnqueteActive;
        }

        public bool EstCloturee()
        {
            return Statut == StatutAffaire.Resolue || Statut == StatutAffaire.NonResolue;
        }

        public int NombreJoursOuverture()
        {
            var dateFin = DateFermeture ?? DateTime.Now;
            return (int)(dateFin - DateOuverture).TotalDays;
        }

        public void Cloturer(StatutAffaire nouveauStatut, string? userId = null)
        {
            if (nouveauStatut != StatutAffaire.Resolue && nouveauStatut != StatutAffaire.NonResolue)
                throw new ArgumentException("Le statut de clôture doit être 'Résolue' ou 'NonRésolue'");

            Statut = nouveauStatut;
            DateFermeture = DateTime.Now;
            DateModification = DateTime.Now;

            if (!string.IsNullOrEmpty(userId))
                ModifieParUserId = userId;
        }

        public void Rouvrir(string? userId = null)
        {
            if (!EstCloturee())
                throw new InvalidOperationException("Seules les affaires clôturées peuvent être rouvertes");

            Statut = StatutAffaire.Ouverte;
            DateFermeture = null;
            DateModification = DateTime.Now;

            if (!string.IsNullOrEmpty(userId))
                ModifieParUserId = userId;
        }

        // Méthode pour générer un numéro d'affaire automatique
        public static string GenererNumeroAffaire(DateTime? date = null)
        {
            var dateRef = date ?? DateTime.Now;
            return $"AFF-{dateRef:yyyy}-{dateRef:MM}{dateRef:dd}-{dateRef:HHmmss}";
        }
    }

    // Enums associés
    public enum StatutAffaire
    {
        [Display(Name = "Ouverte")]
        Ouverte = 0,

        [Display(Name = "Enquete active")]
        EnqueteActive = 1,

        [Display(Name = "Suspendue")]
        Suspendue = 2,

        [Display(Name = "Resolue")]
        Resolue = 3,

        [Display(Name = "Non resolue")]
        NonResolue = 4,

        [Display(Name = "Archivee")]
        Archivee = 5
    }

    public enum Priorite
    {
        [Display(Name = "Faible")]
        Faible = 0,

        [Display(Name = "Moyenne")]
        Moyenne = 1,

        [Display(Name = "Haute")]
        Haute = 2,

        [Display(Name = "Urgente")]
        Urgente = 3
    }

    // Classe d'extension pour les enums
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;

            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}