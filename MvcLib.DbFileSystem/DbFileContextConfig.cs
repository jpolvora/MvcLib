using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace MvcLib.DbFileSystem
{
    public class DbFileContextConfig : DbConfiguration
    {
        public DbFileContextConfig()
        {
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<DbFileContext, DbFileContextMigrationConfiguration>());
            //SetDatabaseInitializer<DbFileContext>(null);
            SetManifestTokenResolver(new MyManifestTokenResolver());
        }

        public class MyManifestTokenResolver : IManifestTokenResolver
        {
            private readonly IManifestTokenResolver _defaultResolver = new DefaultManifestTokenResolver();

            public string ResolveManifestToken(DbConnection connection)
            {
                var sqlConn = connection as SqlConnection;
                return sqlConn != null
                    ? "2008"
                    : _defaultResolver.ResolveManifestToken(connection);
            }
        }
    }
}