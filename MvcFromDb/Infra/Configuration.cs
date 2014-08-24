using System.Data.Entity.Migrations;

namespace MvcFromDb.Infra
{
    internal sealed class DbFileContextMigrationConfiguration : DbMigrationsConfiguration<MvcFromDb.Infra.DbFileContext>
    {
        public DbFileContextMigrationConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(MvcFromDb.Infra.DbFileContext context)
        {
            context.DbFiles.AddOrUpdate(x => x.VirtualPath, new DbFile()
            {
                IsDirectory = true,
                VirtualPath = "/"
            });
        }
    }
}
