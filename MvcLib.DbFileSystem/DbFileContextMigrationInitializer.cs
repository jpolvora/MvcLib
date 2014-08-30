using System.Data.Entity;

namespace MvcLib.DbFileSystem
{
    public class DbFileContextMigrationInitializer : MigrateDatabaseToLatestVersion<DbFileContext, DbFileContextMigrationConfiguration>
    {
        
    }
}