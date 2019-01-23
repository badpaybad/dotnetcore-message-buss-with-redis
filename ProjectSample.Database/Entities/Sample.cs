using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ProjectSample.Database.Entities
{
    [Table("Sample")]
    public class Sample
    {
        public Guid Id { get; set; }
        [StringLength(128)]
        public string Version { get; set; }
        public string JsonData { get; set; }
    }
}
