using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.Models
{
    public static class AffaireExtensions
    {
        public static bool PeutEtreArchivee(this Affaire affaire)
        {
            return affaire.DateFermeture.HasValue &&
                   (affaire.Statut == StatutAffaire.Resolue || affaire.Statut == StatutAffaire.NonResolue) &&
                   affaire.DateFermeture.Value.AddDays(30) <= DateTime.Now; // Exemple: 30 jours après clôture
        }

        public static bool PeutEtreCloturee(this Affaire affaire)
        {
            return affaire.Statut == StatutAffaire.Ouverte ||
                   affaire.Statut == StatutAffaire.EnqueteActive ||
                   affaire.Statut == StatutAffaire.Suspendue;
        }

        public static long CalculerTailleEstimee(this Affaire affaire)
        {
            // Calcul approximatif de la taille de l'affaire (description + preuves)
            long taille = 0;

            // Taille des textes
            taille += (affaire.Titre?.Length ?? 0) * 2; // Unicode = 2 bytes par caractère
            taille += (affaire.Description?.Length ?? 0) * 2;



            return taille;
        }
    }
}