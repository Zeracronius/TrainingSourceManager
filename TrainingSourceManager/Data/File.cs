using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Data
{
    [Table("File")]
    public class File
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected private File()
        {
            Extension = string.Empty;
        }

        internal File(FileInfo file, Source source)
        {
            Extension = file.Extension.ToLower().TrimStart('.');
            Length = file.Length;
            Name = Path.GetFileNameWithoutExtension(file.FullName);

            Byte[] bytes = System.IO.File.ReadAllBytes(file.FullName);
            FileData = new FileData(this, bytes);


            Source = source;
            SourceId = source.Id;
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SourceId { get; set; }

        /// <summary>
        /// File length in bytes.
        /// </summary>
        public long Length { get; set; }

        [StringLength(20)]
        public string Extension { get; set; }

        [StringLength(200)]
        public string Name { get; set; }

        internal FileData? FileData { get; set; }

        [ForeignKey(nameof(SourceId))]
        public Source Source { get; set; }


        public FileInfo? Export(string directoryPath)
        {
            if (FileData == null)
                return null;

            FileInfo file = new FileInfo(Path.Combine(directoryPath, Name) + "." + Extension);

            using (FileStream stream = System.IO.File.Create(file.FullName))
            {
                stream.Write(FileData.Data);
                stream.Flush();
                return file;
            }
        }
    }
}
