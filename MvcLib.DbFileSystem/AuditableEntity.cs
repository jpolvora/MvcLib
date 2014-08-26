using System;
using System.ComponentModel.DataAnnotations;

namespace MvcLib.DbFileSystem
{
    public abstract class AuditableEntity
    {
        public DateTime Created { get; set; }

        public DateTime? Modified { get; set; }
    }

    public abstract class AuditableEntity<TKey> : AuditableEntity
    {
        [Key]
        public TKey Id { get; set; }
    }
}