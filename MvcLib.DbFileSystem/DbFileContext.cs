﻿using System;
using System.Data.Entity;
using System.Diagnostics;
using MvcLib.Common;

namespace MvcLib.DbFileSystem
{
    public class DbFileContext : DbContext
    {
        public DbSet<DbFile> DbFiles { get; set; }

        public static void Initialize()
        {
            using (var db = new DbFileContext())
            {
                db.Database.Initialize(false);
            }
        }

        static DbFileContext()
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<DbFileContext, DbFileContextMigrationConfiguration>());
        }

        public DbFileContext()
            : base(Config.ValueOrDefault("CustomContextKey", "DbFileContext"))
        {
            var cfg = Config.ValueOrDefault("CustomDbFileContextVerbose", true);
            if (cfg)
            {
                Database.Log = Log;
            }
        }

        static void Log(string str)
        {
            if (str.StartsWith("-- Completed"))
                Trace.WriteLine(str.Replace(Environment.NewLine, ""));
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