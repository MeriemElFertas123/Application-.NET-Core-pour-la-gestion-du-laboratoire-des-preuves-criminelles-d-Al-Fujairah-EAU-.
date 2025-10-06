using System;

namespace WebApplication1.Models.Envoi
{
    public class EchantillonDisponibleModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool DejaEnvoye { get; set; }
    }
}