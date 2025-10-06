// ViewModels/Envoi/VerificationEchantillonViewModel.cs
using System.Collections.Generic;

namespace WebApplication1.Models.Envoi
{
    public class VerificationEchantillonViewModel
    {
        public int EnvoiEchantillonId { get; set; }
        public EnvoiEchantillonReceptionDto EchantillonInfo { get; set; }
        public List<CritereVerification> CriteresVerification { get; set; } = new List<CritereVerification>();
        public string NotesVerification { get; set; }
        public bool EstAccepte { get; set; }
        public string ActionSiRefus { get; set; }
        public string NotesRefus { get; set; }
    }

    public class CritereVerification
    {
        public string Nom { get; set; }
        public string ValeurAttendue { get; set; }
        public string ValeurConstatee { get; set; }
        public bool EstVerifie { get; set; }
        public bool EstConforme { get; set; }
        public string Observations { get; set; }
    }
}