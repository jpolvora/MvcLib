using System;
using System.Data.Entity;

namespace MvcFromDb.Infra
{
    public class DbFileContext : DbContext
    {
        public DbSet<DbFile> DbFiles { get; set; }

        public static void Initialize() { }

        static DbFileContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DbFileContext, DbFileContextMigrationConfiguration>());
        }

        public DbFileContext()
            : base("name=DbFileContext")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            var auditables = ChangeTracker.Entries<AuditableEntity>();
            foreach (var auditable in auditables)
            {
                switch (auditable.State)
                {
                    case EntityState.Added:
                        auditable.Entity.Created = DateTime.Now;
                        auditable.Entity.Modified = null;
                        break;
                    case EntityState.Modified:
                        auditable.Property(x => x.Created).IsModified = false;
                        auditable.Entity.Modified = DateTime.Now;
                        break;
                }
            }

            return base.SaveChanges();
        }
    }
}