using System.Data.Entity.Migrations;
using System.Diagnostics;
using MvcLib.Common;
using MvcLib.Common.Configuration;

namespace MvcLib.DbFileSystem
{
    /// <summary>
    /// Changed to public in order to run updata-database in the main project (i.e., the project that referenced this library)
    /// </summary>
    public class DbFileContextMigrationConfiguration : DbMigrationsConfiguration<DbFileContext>
    {
        public DbFileContextMigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;

            ContextKey = BootstrapperSection.Instance.DbFileContext.MigrationKey;

            Trace.TraceInformation("Running Migrations... {0}", this);
        }

        protected override void Seed(DbFileContext context)
        {
            using (DisposableTimer.StartNew("Seeding DbFileContext"))
            {
                context.DbFiles.AddOrUpdate(x => x.VirtualPath, new DbFile()
                {
                    IsDirectory = true,
                    VirtualPath = "/"
                });
            }
        }
    }
}
