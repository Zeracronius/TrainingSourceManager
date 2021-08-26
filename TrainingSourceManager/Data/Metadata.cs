using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Data
{
    [Table("Metadata")]
    internal class Metadata
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private protected Metadata(MetadataType type, string value)
        {
            Type = type;
            Value = value;
        }

        public Metadata(MetadataType type, string value, Source source)
            : this(type, value)
        {
            Source = source;
            SourceId = source.Id;
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SourceId { get; set; }
        
        public MetadataType Type { get; set; }

        [StringLength(500)]
        public string Value { get; set; }


        
        [ForeignKey(nameof(SourceId))]
        public Source Source { get; set; }
    }
}
