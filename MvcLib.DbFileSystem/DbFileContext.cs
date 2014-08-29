using System;
using System.Data.Entity;
using System.Diagnostics;
using MvcLib.Common;

namespace MvcLib.DbFileSystem
{
    public class DbFileContext : DbContext
    {
        public DbSet<DbFile> DbFiles { get; set; }

        public static string ConnectionStringKey { get; private set; }
        public static bool Verbose { get; private set; }

        public static void Initialize()
        {
            using (var db = new DbFileContext())
            {
                using (DisposableTimer
                    .StartNew("Initializing DbFileContext using ConnectionString: {0}"
                    .Fmt(db.Database.Connection.ConnectionString)))
                {
                    db.Database.Initialize(false);
                }
            }
        }

        static DbFileContext()
        {
            ConnectionStringKey = Config.ValueOrDefault("DbFileContextKey", "DbFileContext");
            Verbose = Config.ValueOrDefault("DbFileContextVerbose", true);
            //Database.SetInitializer(new MigrateDatabaseToLatestVersion<DbFileContext, DbFileContextMigrationConfiguration>());
            System.Data.Entity.Database.SetInitializer(new NullDatabaseInitializer<DbFileContext>());
        }

        public DbFileContext()
            : this(ConnectionStringKey)
        {
        }

        public DbFileContext(string connStrKey)
            : base(connStrKey)
        {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;

            if (Verbose)
            {
                Database.Log = Log;
            }
        }

        static void Log(string str)
        {
            if (str.StartsWith("-- Completed"))
                Trace.TraceInformation("[DbFileContext]:{0}", str.Replace(Environment.NewLine, ""));
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
                        auditable.Entity.Created = DateTime.UtcNow;
                        auditable.Entity.Modified = null;
                        break;
                    case EntityState.Modified:
                        auditable.Property(x => x.Created).IsModified = false;
                        auditable.Entity.Modified = DateTime.UtcNow;
                        break;
                }
            }

            return base.SaveChanges();
        }
    }
}