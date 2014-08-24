using System.Data.Entity.Migrations;
using System.Diagnostics;

namespace MvcFromDb.Infra.Entities
{
    internal sealed class DbFileContextMigrationConfiguration : DbMigrationsConfiguration<DbFileContext>
    {
        public DbFileContextMigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;

            Trace.TraceInformation("Running Migrations... {0}", this);
        }

        protected override void Seed(DbFileContext context)
        {
            context.DbFiles.AddOrUpdate(x => x.VirtualPath, new DbFile()
            {
                IsDirectory = true,
                VirtualPath = "/"
            });
        }
    }
}
