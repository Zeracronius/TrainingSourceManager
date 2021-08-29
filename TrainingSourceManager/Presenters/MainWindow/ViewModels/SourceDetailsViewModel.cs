using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.MainWindow.ViewModels
{
    public class SourceDetailsViewModel
    {
        internal Data.Source Source { get; private set; }


        public string Name { get; private set; }

        public ObservableCollection<FileViewModel> Files { get; private set; }
        public ObservableCollection<string> Tags { get; private set; }

        public SourceDetailsViewModel(Data.Source source)
        {
            Source = source;
            Name = string.Empty;
            Tags = new ObservableCollection<string>();
            Files = new ObservableCollection<FileViewModel>();
        }

        public void LoadData()
        {
            Name = Source.Name;
            Files = new ObservableCollection<FileViewModel>(Source.Files.Select(x => new FileViewModel(x)));
            Tags = new ObservableCollection<string>(Source.Metadata.Where(x => x.Type == Data.MetadataType.Tag).Select(x => x.Value));
        }
    }
}
