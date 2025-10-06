// ViewModels/CloturerAffaireViewModel.cs
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Models;

namespace WebApplication1.ViewModels
{
    public class CloturerAffaireViewModel
    {
        // Ajoutez cette propriété manquante
        public List<Affaire> Affaires { get; set; }

        public SelectList Enqueteurs { get; set; }
        public int? TotalAffaires { get; set; }
        public int? AffairesPretes { get; set; }
        public int? AffairesEnCours { get; set; }
        public int? AffairesBloquees { get; set; }
        public string Statut { get; set; }
        public string Priorite { get; set; }
        public string Recommandations { get; set; }
        public DateTime? DateOuverture { get; set; }
        public int? Enqueteur { get; set; }
        public string SearchText { get; set; }
    }

    public class AffaireDetailsViewModel
    {
        public int Id { get; set; }
        public string NumeroAffaire { get; set; }
        public string Titre { get; set; }
        public DateTime DateCreation { get; set; }
        public string Enqueteur { get; set; }
        public string Statut { get; set; }
        public int NombreEchantillons { get; set; }
        public int NombreAnalyses { get; set; }
        public int NombreRapports { get; set; }
        public int AnalysesTerminees { get; set; }
        public int RapportsValides { get; set; }
        public List<string> Warnings { get; set; } = new List<string>();
        public bool PeutEtreClotree { get; set; }
    }
}