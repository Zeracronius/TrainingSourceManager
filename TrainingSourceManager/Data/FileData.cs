using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Data
{

    [Table("FileData")]
    internal class FileData
    {
        protected private FileData()
        {
            Data = Array.Empty<byte>();
        }

        public FileData(File file, byte[] data)
        {
            FileId = file.Id;
            Data = data;
        }

        [Key]
        public int FileId { get; set; }
        //public int FileId { get; set; }
        public byte[] Data { get; set; }
    }
}
