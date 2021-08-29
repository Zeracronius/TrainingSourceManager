using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.MainWindow.ViewModels
{
    public class FileViewModel
    {
        public FileViewModel(Data.File file)
        {
            Name = file.Name;
            Size = (file.Length / 1024D).ToString("N1") + " KB";
            Type = file.Extension;
        }

        public string Name { get; }
        public string Size { get; }
        public string Type { get; }
    }
}
