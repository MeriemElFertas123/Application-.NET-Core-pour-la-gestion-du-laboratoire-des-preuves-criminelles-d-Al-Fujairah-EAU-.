using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class AnalysesAutorisees
    {
        [Key]
        public int Id { get; set; } // Ajout d’un Id car une table sans clé n’est pas mappée correctement

        [Required]
        public int EnqueteurId { get; set; } // Clé étrangère si table Enqueteurs existe

        [Required]
        [StringLength(100)]
        public string TypeAnalyse { get; set; }

        public virtual Enqueteur Enqueteur { get; set; } // à décommenter si table Enqueteur existe
    }
}
