using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Models.Analyse;
using WebApplication1.Models.Emploi;
using WebApplication1.Models.Envoi;
using WebApplication1.Models.Stockage;

namespace WebApplication1.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Enqueteur> Enqueteurs { get; set; }
        public DbSet<Analyste> Analystes { get; set; }
        public DbSet<Archive> Archives { get; set; }
        //public DbSet<Preuve> Preuves { get; set; }
        public DbSet<RapportAnalyse> RapportAnalyses { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Affaire> Affaires { get; set; }
        public DbSet<RapportCloture> RapportCloture { get; set; }
       // public DbSet<PreuveAnalysee> PreuvesAnalysees { get; set; }
        public DbSet<TypesEchantillons> TypesEchantillons { get; set; }
        public DbSet<TypesAnalyses> TypesAnalyses { get; set; }
        public DbSet<Technicien> Techniciens { get; set; }
        //public DbSet<RapportsNonCorrespondance> RapportsNonCorrespondance { get; set; }
        //public DbSet<TestsCorrespondance> TestsCorrespondance { get; set; }
       // public DbSet<CriteresCorrespondance> CriteresCorrespondance { get; set; }
        //public DbSet<DemandesAnalyses> DemandesAnalyses { get; set; }
        //
        //public DbSet<PreuveTransmise> PreuvesTransmises { get; set; }

        // Emplois du temps
       // public DbSet<EmploiTemps> EmploisTemps { get; set; }
        public DbSet<Personnel> Personnels { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<AffectationPersonnel> AffectationsPersonnel { get; set; }
        public DbSet<AbsenceConge> AbsencesConges { get; set; }

        // Stockage
        public DbSet<Stockage> Stockages { get; set; }
        public DbSet<ZoneStockage> ZoneStockages { get; set; }
        public DbSet<Emplacement> Emplacements { get; set; }
        public DbSet<Echantillon> Echantillons { get; set; }
        public DbSet<AttributionAnalyse> AttributionAnalyses { get; set; }

        // Envoi
        public DbSet<Envoi> Envois { get; set; }
        public DbSet<EnvoiComplet> EnvoiComplets { get; set; }
        public DbSet<EnvoiEchantillon> EnvoiEchantillons { get; set; }
        public DbSet<VerificationEchantillon> VerificationEchantillons { get; set; }
        public DbSet<RapportNonConformite> RapportsNonConformite { get; set; }

        // Analyse
        public DbSet<Analyse> Analyses { get; set; }
        public DbSet<HistoriqueAffaire> HistoriqueAffaires { get; set; }
        public DbSet <EmploiTemps> EmploisTemps { get; internal set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Exemple migration EF6 → EF Core
            modelBuilder.Entity<Envoi>()
                .ToTable("Envois")
                .HasKey(e => e.Id);

            modelBuilder.Entity<Envoi>()
                .Property(e => e.TypeAnalyseDemandee)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Envoi>()
                .HasOne(e => e.Affaire)
                .WithMany()
                .HasForeignKey(e => e.AffaireId)
                .OnDelete(DeleteBehavior.Restrict);

            // Exemple pour clé composite (UserPermission)
            modelBuilder.Entity<UserPermission>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            // Exemple remplacement cascade delete
            modelBuilder.Entity<Personnel>()
                .HasOne(p => p.Service)
                .WithMany(s => s.Personnels)
                .HasForeignKey(p => p.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);
             modelBuilder.Entity<AbsenceConge>()
                .HasOne(a => a.Personnel)
                .WithMany(p => p.AbsencesConges) // ou .WithMany() si Personnel n'a pas de collection
                .HasForeignKey(a => a.PersonnelId) // clé étrangère dans AbsenceConge
                .OnDelete(DeleteBehavior.Restrict); // ou Cascade selon ton besoin
                    // Exemple d’index avec nom
            modelBuilder.Entity<Echantillon>()
                .HasIndex(e => e.NumeroEchantillon)
                .IsUnique()
                .HasDatabaseName("IX_Echantillon_NumeroEchantillon");
            modelBuilder.Entity<Echantillon>()
                .HasOne(e => e.Stockage)
                .WithOne(s => s.Echantillon)
                .HasForeignKey<Echantillon>(e => e.StockageId) // <-- côté dépendant
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttributionAnalyse>()
                .HasOne(a => a.Echantillon)
               .WithMany(e => e.AttributionAnalyses)
               .HasForeignKey(a => a.EchantillonId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RapportAnalyse>()
     .HasOne(r => r.preuve)
     .WithMany(e => e.Rapports) // <-- ici la collection existante
     .HasForeignKey(r => r.idPreuve)
     .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
