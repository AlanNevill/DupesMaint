namespace DupesMaintConsole
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("DupesAction")]
    public partial class DupesAction
    {
        [Key]
        [StringLength(200)]
        public string TheFileName { get; set; }

        [Required]
        [StringLength(200)]
        public string DuplicateOf { get; set; }

        [Required]
        [StringLength(200)]
        public string SHA { get; set; }

        [Required]
        [StringLength(10)]
        public string FileExt { get; set; }

        public int FileSize { get; set; }

        [Column(TypeName = "smalldatetime")]
        public DateTime FileCreateDt { get; set; }

        [Required]
        [StringLength(1)]
        public string OneDriveRemoved { get; set; }

        [Required]
        [StringLength(1)]
        public string GooglePhotosRemoved { get; set; }
    }
}
