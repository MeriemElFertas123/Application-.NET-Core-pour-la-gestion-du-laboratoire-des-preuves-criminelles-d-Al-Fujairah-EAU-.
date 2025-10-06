using System;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Models
{
    public class Archive
    {
        [Key]
        public int idArchive { get; set; }

        public DateTime dateArchivage { get; set; }

        public string emplacementPhysique { get; set; }

        public string codeQR { get; set; } // le même que celui de la preuve

        public int idPreuve { get; set; }
        public virtual Echantillon preuve { get; set; }
    }
}
