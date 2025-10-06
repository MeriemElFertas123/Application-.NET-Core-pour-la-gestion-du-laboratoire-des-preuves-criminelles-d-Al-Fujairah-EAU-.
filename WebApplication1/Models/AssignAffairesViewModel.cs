using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class AssignAffaireViewModel
    {
        [Required(ErrorMessage = "Veuillez sélectionner au moins une affaire")]
        [Display(Name = "Affaires à assigner")]
        public List<int> AffairesSelectionnees { get; set; } = new List<int>();

        [Required(ErrorMessage = "Veuillez sélectionner un enquêteur")]
        [Display(Name = "Enquêteur")]
        public int EnqueteurId { get; set; }

        public List<Affaire> AffairesDisponibles { get; set; } = new List<Affaire>();
        public List<Enqueteur> Enqueteurs { get; set; } = new List<Enqueteur>();
    }
}