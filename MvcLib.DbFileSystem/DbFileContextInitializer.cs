using System.Data.Entity;

namespace MvcLib.DbFileSystem
{
    public class DbFileContextInitializer : MigrateDatabaseToLatestVersion<DbFileContext, DbFileContextMigrationConfiguration>
    {
        
    }
}