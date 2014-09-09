using System;
using System.Data.Entity;
using System.Diagnostics;
using MvcLib.Common.Configuration;

namespace MvcLib.DbFileSystem
{
    public class DbFileContext : DbContext
    {
        private static bool _initialized;

        public DbSet<DbFile> DbFiles { get; set; }

        public static string ConnectionStringKey { get; private set; }
        public static bool Verbose { get; private set; }

        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            using (var db = new DbFileContext())
            {
                Trace.TraceInformation("Connection String: {0}", db.Database.Connection.ConnectionString);
                db.Database.Initialize(false);
            }
        }

        static DbFileContext()
        {
            ConnectionStringKey = BootstrapperSection.Instance.DbFileContext.ConnectionStringKey;
            Verbose = BootstrapperSection.Instance.DbFileContext.Verbose;
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
            {
                Trace.Indent();
                Trace.TraceInformation("[DbFileContext]:{0}", str.Replace(Environment.NewLine, ""));
                Trace.Unindent();
            }
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