using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcLib.DbFileSystem
{
    public class DbFile : AuditableEntity<int>
    {
        public DbFile()
        {
            Children = new HashSet<DbFile>();

            Bytes = new byte[0];
        }

        [ForeignKey("ParentId")]
        public DbFile Parent { get; set; }
        public int? ParentId { get; set; }

        /// <summary>
        /// Indica se o registro refere-se a um diretório. 
        /// </summary>
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Caminho completo do arquivo ou diretório
        /// </summary>
        [Required, StringLength(255), Index(IsUnique = true)]
        public string VirtualPath { get; set; }

        /// <summary>
        /// Nome do arquivo ou diretório
        /// </summary>
        [StringLength(512)]
        public string Name { get; set; }

        /// <summary>
        /// Extensão do arquivo
        /// </summary>
        [StringLength(8)]
        public string Extension { get; set; }

        /// <summary>
        /// Conteúdo do arquivo
        /// </summary>
        public byte[] Bytes { get; set; }

        [MaxLength]
        public string Texto { get; set; }

        /// <summary>
        /// Indica se o arquivo é oculto. REGRA: O VPP não enxerga arquivos ocultos
        /// </summary>
        public bool IsHidden { get; set; }

        public bool IsBinary { get; set; }

        /// <summary>
        /// Referências aos arquivos do diretório atual
        /// </summary>
        public ICollection<DbFile> Children { get; set; }

    }
}
