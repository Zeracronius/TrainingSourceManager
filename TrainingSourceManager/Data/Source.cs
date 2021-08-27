using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Data
{
    [Table("Source")]
    public class Source
    {
        public Source(string name)
        {
            Metadata = new List<Metadata>();
            Files = new List<File>();
            Name = name;
        }

        [Key]
        public int Id { get; set; }

        [StringLength(500)]
        public string Name { get; set; }

        [InverseProperty(nameof(Data.Metadata.Source))]
        public ICollection<Metadata> Metadata { get; set; }

        [InverseProperty(nameof(Data.File.Source))]
        public ICollection<File> Files { get; set; }

        public Metadata AddMetadata(MetadataType type, string value)
        {
            Metadata meta = new Data.Metadata(type, value, this);
            Metadata.Add(meta);
            return meta;
        }

        public File AddFile(System.IO.FileInfo fileInfo)
        {
            File file = new Data.File(fileInfo, this);
            Files.Add(file);
            return file;
        }
    }
}
