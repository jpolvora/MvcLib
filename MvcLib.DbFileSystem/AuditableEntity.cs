using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcLib.DbFileSystem
{
    public abstract class AuditableEntity
    {
        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }

        [NotMapped]
        public DateTime LastWriteUtc
        {
            get
            {
                return Modified.HasValue
                    ? Modified.Value.ToUniversalTime()
                    : Created.ToUniversalTime();
            }

        }
    }

    public abstract class AuditableEntity<TKey> : AuditableEntity
    {
        [Key]
        public TKey Id { get; set; }
    }
}