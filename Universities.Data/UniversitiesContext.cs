using Universities.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Universities.Data
{
    public class UniversitiesContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<AcadPerson> AcadPersonnel { get; set; }
        public DbSet<DuplicateDocument> DuplicateDocuments { get; set; }
        public DbSet<IncompleteDocument> IncompleteDocuments { get; set; }
        public DbSet<RegexPattern> RegexPatterns { get; set; }
        public string ConnectionString { get; set; }

        public UniversitiesContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public UniversitiesContext(DbContextOptions options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                MariaDbServerVersion mariaDbServer = new MariaDbServerVersion(ServerVersion.AutoDetect(ConnectionString));// new Version(5, 5, 68));
                optionsBuilder.UseMySql(ConnectionString, mariaDbServer, builder => { builder.EnableRetryOnFailure(5); });
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Transaction>().HasKey(x => new { x.GuestId, x.ReservationId });
            // Solve cyclic dependency ON DELETE....
            //modelBuilder.Entity<Transaction>()
            //    .HasOne(x => x.Guest)
            //    .WithMany(x => x.Transactions)
            //    .HasForeignKey(x => x.GuestId)
            //    .OnDelete(DeleteBehavior.Restrict);
            //modelBuilder.Entity<Transaction>()
            //    .HasOne(x => x.Reservation)
            //    .WithMany(x => x.Transactions)
            //    .HasForeignKey(x => x.ReservationId)
            //    .OnDelete(DeleteBehavior.Restrict);
            //modelBuilder.Entity<Guest>()
            //    .HasOne(g => g.GuestReferrer)
            //    .WithMany(g => g.GuestReferrals)
            //    .HasForeignKey(g => g.GuestReferrerId)
            //    .OnDelete(DeleteBehavior.Restrict);
        }
    }
}