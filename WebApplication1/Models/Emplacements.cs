using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public class Emplacements
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Zone { get; set; }  // Ex: Frigo, Armoire, Rayonnage

        [Required]
        public int Rangee { get; set; }   // Rangée dans la zone

        [Required]
        [StringLength(10)]
        public string Etagere { get; set; }  // Exemple : A, B, C

        [Required]
        [StringLength(100)]
        public string TypeStockage { get; set; }  // Ex: Biologique, Chimique, Matériel

        public bool Occupe { get; set; }  // Indique si l’emplacement est occupé ou non
    }
}