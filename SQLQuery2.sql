-- Vider les tables en respectant les contraintes FK
DELETE FROM AffectationsPersonnel;
DELETE FROM EmploisTemps;

-- Réinitialiser les compteurs IDENTITY
DBCC CHECKIDENT ('EmploisTemps', RESEED, 0);
DBCC CHECKIDENT ('AffectationsPersonnel', RESEED, 0);

-- Insertion des emplois du temps pour les 4 prochaines semaines
DECLARE @CurrentDate DATE = GETDATE();
DECLARE @WeekNumber INT, @Year INT, @StartDate DATE, @EndDate DATE;

-- Semaine 1 (semaine courante)
SET @WeekNumber = DATEPART(WEEK, @CurrentDate);
SET @Year = YEAR(@CurrentDate);
SET @StartDate = DATEADD(DAY, 1 - DATEPART(WEEKDAY, @CurrentDate), @CurrentDate);
SET @EndDate = DATEADD(DAY, 7, @StartDate);

INSERT INTO EmploisTemps (NumeroSemaine, Annee, DateDebut, DateFin, Statut, Commentaires, DateCreation, CreePar)
VALUES (@WeekNumber, @Year, @StartDate, @EndDate, 'Actif', 'Emploi du temps semaine courante', GETDATE(), 'System');

-- Semaines suivantes
INSERT INTO EmploisTemps (NumeroSemaine, Annee, DateDebut, DateFin, Statut, Commentaires, DateCreation, CreePar)
VALUES 
(@WeekNumber + 1, @Year, DATEADD(WEEK, 1, @StartDate), DATEADD(WEEK, 1, @EndDate), 'Brouillon', 'Emploi du temps semaine suivante', GETDATE(), 'System'),
(@WeekNumber + 2, @Year, DATEADD(WEEK, 2, @StartDate), DATEADD(WEEK, 2, @EndDate), 'Brouillon', 'Emploi du temps semaine +2', GETDATE(), 'System'),
(@WeekNumber + 3, @Year, DATEADD(WEEK, 3, @StartDate), DATEADD(WEEK, 3, @EndDate), 'Brouillon', 'Emploi du temps semaine +3', GETDATE(), 'System');

-- Insertion des affectations pour chaque service et personnel
DECLARE @EmploiTempsId INT, @PersonnelId INT, @ServiceId INT;
DECLARE @Jour DATE, @Equipe VARCHAR(20), @HeureDebut TIME, @HeureFin TIME;

-- Récupérer l'ID du premier emploi du temps (semaine courante)
SELECT @EmploiTempsId = Id FROM EmploisTemps WHERE Statut = 'Actif';

-- Affectations pour le Service ADN (ServiceId = 1)
SET @ServiceId = 1;
SET @Equipe = 'Matin';

-- Personnel ADN (IDs 1-7)
SET @PersonnelId = 1;
WHILE @PersonnelId <= 7
BEGIN
    SET @Jour = @StartDate;
    WHILE @Jour < @EndDate
    BEGIN
        -- Ne pas planifier le weekend
        IF DATEPART(WEEKDAY, @Jour) NOT IN (1, 7)
        BEGIN
            SET @HeureDebut = '08:00';
            SET @HeureFin = '16:00';
            
            INSERT INTO AffectationsPersonnel (EmploiTempsId, PersonnelId, ServiceId, Jour, Equipe, HeureDebut, HeureFin, DateCreation)
            VALUES (@EmploiTempsId, @PersonnelId, @ServiceId, @Jour, @Equipe, @HeureDebut, @HeureFin, GETDATE());
        END
        
        SET @Jour = DATEADD(DAY, 1, @Jour);
    END
    
    SET @PersonnelId = @PersonnelId + 1;
END

-- Affectations pour le Service Toxicologie (ServiceId = 2)
SET @ServiceId = 2;
SET @Equipe = 'Matin';

-- Personnel Toxicologie (IDs 8-12)
SET @PersonnelId = 8;
WHILE @PersonnelId <= 12
BEGIN
    SET @Jour = @StartDate;
    WHILE @Jour < @EndDate
    BEGIN
        IF DATEPART(WEEKDAY, @Jour) NOT IN (1, 7)
        BEGIN
            SET @HeureDebut = '09:00';
            SET @HeureFin = '17:00';
            
            INSERT INTO AffectationsPersonnel (EmploiTempsId, PersonnelId, ServiceId, Jour, Equipe, HeureDebut, HeureFin, DateCreation)
            VALUES (@EmploiTempsId, @PersonnelId, @ServiceId, @Jour, @Equipe, @HeureDebut, @HeureFin, GETDATE());
        END
        
        SET @Jour = DATEADD(DAY, 1, @Jour);
    END
    
    SET @PersonnelId = @PersonnelId + 1;
END

-- Affectations pour le Service Balistique (ServiceId = 3)
SET @ServiceId = 3;
SET @Equipe = 'Matin';

-- Personnel Balistique (IDs 13-15)
SET @PersonnelId = 13;
WHILE @PersonnelId <= 15
BEGIN
    SET @Jour = @StartDate;
    WHILE @Jour < @EndDate
    BEGIN
        IF DATEPART(WEEKDAY, @Jour) NOT IN (1, 7)
        BEGIN
            SET @HeureDebut = '08:30';
            SET @HeureFin = '16:30';
            
            INSERT INTO AffectationsPersonnel (EmploiTempsId, PersonnelId, ServiceId, Jour, Equipe, HeureDebut, HeureFin, DateCreation)
            VALUES (@EmploiTempsId, @PersonnelId, @ServiceId, @Jour, @Equipe, @HeureDebut, @HeureFin, GETDATE());
        END
        
        SET @Jour = DATEADD(DAY, 1, @Jour);
    END
    
    SET @PersonnelId = @PersonnelId + 1;
END

-- Affectations pour le Service Empreintes (ServiceId = 4)
SET @ServiceId = 4;
SET @Equipe = 'Matin';

-- Personnel Empreintes (IDs 16-21)
SET @PersonnelId = 16;
WHILE @PersonnelId <= 21
BEGIN
    SET @Jour = @StartDate;
    WHILE @Jour < @EndDate
    BEGIN
        IF DATEPART(WEEKDAY, @Jour) NOT IN (1, 7)
        BEGIN
            SET @HeureDebut = '08:00';
            SET @HeureFin = '16:00';
            
            INSERT INTO AffectationsPersonnel (EmploiTempsId, PersonnelId, ServiceId, Jour, Equipe, HeureDebut, HeureFin, DateCreation)
            VALUES (@EmploiTempsId, @PersonnelId, @ServiceId, @Jour, @Equipe, @HeureDebut, @HeureFin, GETDATE());
        END
        
        SET @Jour = DATEADD(DAY, 1, @Jour);
    END
    
    SET @PersonnelId = @PersonnelId + 1;
END

-- Ajouter quelques variations d'équipes pour plus de réalisme
UPDATE AffectationsPersonnel 
SET Equipe = 'Après-midi', 
    HeureDebut = '16:00', 
    HeureFin = '00:00'
WHERE PersonnelId IN (2, 5, 9, 12, 14, 17, 20) 
AND Jour = DATEADD(DAY, 2, @StartDate);

UPDATE AffectationsPersonnel 
SET Equipe = 'Nuit', 
    HeureDebut = '00:00', 
    HeureFin = '08:00'
WHERE PersonnelId IN (3, 6, 10, 15, 18, 21) 
AND Jour = DATEADD(DAY, 4, @StartDate);

-- Afficher le résultat
SELECT 'Emplois du temps créés :' AS Message;
SELECT * FROM EmploisTemps;

SELECT 'Affectations créées :' AS Message;
SELECT COUNT(*) AS NombreAffectations FROM AffectationsPersonnel;