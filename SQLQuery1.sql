-- Réinitialiser l'identité (facultatif)
DBCC CHECKIDENT ('EmploisTemps', RESEED, 0);

INSERT INTO EmploisTemps
(NumeroSemaine, Annee, DateDebut, DateFin, Statut, Commentaires, DateCreation, DateModification, DateApprobation, CreePar, ApprouvePar, ModifiePar)
VALUES
(37, 2025, '2025-09-15', '2025-09-21', 'Draft', 'Création initiale de la semaine 37', GETDATE(), NULL, NULL, 'admin', '', ''),

(38, 2025, '2025-09-22', '2025-09-28', 'Pending', 'En attente de validation par le directeur', GETDATE(), NULL, NULL, 'admin', '', ''),

(39, 2025, '2025-09-29', '2025-10-05', 'Approved', 'Validé et publié', GETDATE(), GETDATE(), GETDATE(), 'admin', 'directeur', ''),

(40, 2025, '2025-10-06', '2025-10-12', 'Cancelled', 'Annulé en raison de jours fériés', GETDATE(), GETDATE(), NULL, 'admin', '', 'manager');
