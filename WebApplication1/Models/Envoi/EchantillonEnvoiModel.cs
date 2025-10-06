using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models.Envoi
{
    public class EchantillonEnvoiModel
    {
        public int Id { get; set; }

        public int BonEnvoiId { get; set; }

        public int EchantillonId { get; set; }

        [StringLength(100)]
        public string CodeEchantillon { get; set; }

        [StringLength(100)]
        public string TypeAnalyse { get; set; }

        // ===== CHAMPS CARACTÉRISTIQUES MODIFIÉS =====

        /// <summary>
        /// Poids de l'échantillon en grammes (obligatoire)
        /// </summary>
        [Required(ErrorMessage = "Le poids est obligatoire")]
        [Range(0.001, 99999.999, ErrorMessage = "Le poids doit être compris entre 0.001g et 99999.999g")]
        public decimal Poids { get; set; }

        /// <summary>
        /// Conditions de stockage de l'échantillon (obligatoire)
        /// </summary>
        [Required(ErrorMessage = "Les conditions de stockage sont obligatoires")]
        [StringLength(200, ErrorMessage = "Les conditions de stockage ne peuvent pas dépasser 200 caractères")]
        public string ConditionsStockage { get; set; }

        /// <summary>
        /// Couleur de l'échantillon (optionnel)
        /// </summary>
        [StringLength(100, ErrorMessage = "La couleur ne peut pas dépasser 100 caractères")]
        public string Couleur { get; set; }

        /// <summary>
        /// Type d'emballage de l'échantillon (obligatoire)
        /// </summary>
        [Required(ErrorMessage = "Le type d'emballage est obligatoire")]
        [StringLength(100, ErrorMessage = "Le type d'emballage ne peut pas dépasser 100 caractères")]
        public string Emballage { get; set; }

        /// <summary>
        /// Emplacement de stockage de l'échantillon (optionnel)
        /// </summary>
        [StringLength(200, ErrorMessage = "L'emplacement ne peut pas dépasser 200 caractères")]
        public string Emplacement { get; set; }

        // ===== AUTRES CHAMPS =====

        /// <summary>
        /// Nombre de pièces dans l'échantillon
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Le nombre de pièces doit être entre 1 et 1000")]
        public int NombrePieces { get; set; } = 1;

        /// <summary>
        /// État de l'échantillon
        /// </summary>
        [StringLength(100)]
        public string Etat { get; set; }

        /// <summary>
        /// Observations supplémentaires (optionnel)
        /// </summary>
        [StringLength(1000, ErrorMessage = "Les observations ne peuvent pas dépasser 1000 caractères")]
        public string Observations { get; set; }

        /// <summary>
        /// Code QR généré pour l'échantillon
        /// </summary>
        [StringLength(200)]
        public string QRCode { get; set; }

        // ===== CHAMPS POUR FICHIER JOINT =====

        /// <summary>
        /// Nom du fichier joint
        /// </summary>
        [StringLength(255)]
        public string FichierNom { get; set; }

        /// <summary>
        /// Type MIME du fichier joint
        /// </summary>
        [StringLength(100)]
        public string FichierType { get; set; }

        /// <summary>
        /// Taille du fichier joint en bytes
        /// </summary>
        public int? FichierTaille { get; set; }

        /// <summary>
        /// Contenu du fichier en Base64
        /// </summary>
        public string FichierContenu { get; set; }

        // ===== CHAMPS D'AUDIT =====

        /// <summary>
        /// Date de création de l'enregistrement
        /// </summary>
        public DateTime DateCreation { get; set; }

        /// <summary>
        /// Date d'ajout de l'échantillon à l'envoi
        /// </summary>
        public DateTime DateAjout { get; set; }

        /// <summary>
        /// Utilisateur qui a créé l'enregistrement
        /// </summary>
        [StringLength(100)]
        public string CreePar { get; set; }

        /// <summary>
        /// Date de dernière modification
        /// </summary>
        public DateTime? DateModification { get; set; }

        /// <summary>
        /// Utilisateur qui a modifié en dernier
        /// </summary>
        [StringLength(100)]
        public string ModifiePar { get; set; }

        // ===== PROPRIÉTÉS CALCULÉES =====

        /// <summary>
        /// Indique si l'échantillon a un fichier joint
        /// </summary>
        public bool AFichierJoint => !string.IsNullOrEmpty(FichierNom);

        /// <summary>
        /// Taille du fichier formatée en texte lisible
        /// </summary>
        public string FichierTailleFormatee
        {
            get
            {
                if (!FichierTaille.HasValue) return "";

                if (FichierTaille < 1024)
                    return $"{FichierTaille} B";
                else if (FichierTaille < 1024 * 1024)
                    return $"{FichierTaille / 1024:F1} KB";
                else
                    return $"{FichierTaille / (1024 * 1024):F1} MB";
            }
        }

        public bool EstConforme { get; internal set; }
        public object Description { get; internal set; }

        /// <summary>
        /// Validation personnalisée
        /// </summary>
        public bool EstValide(out List<string> erreurs)
        {
            erreurs = new List<string>();

            if (Poids <= 0 || Poids > 99999.999m)
                erreurs.Add("Le poids doit être compris entre 0.001g et 99999.999g");

            if (string.IsNullOrWhiteSpace(ConditionsStockage))
                erreurs.Add("Les conditions de stockage sont obligatoires");

            if (string.IsNullOrWhiteSpace(Emballage))
                erreurs.Add("Le type d'emballage est obligatoire");

            return erreurs.Count == 0;
        }
    }
}