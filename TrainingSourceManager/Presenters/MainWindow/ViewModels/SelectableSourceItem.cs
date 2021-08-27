using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.MainWindow.ViewModels
{
    public class SelectableSourceItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        readonly Data.Source _source;
        private bool _selected;

        public SelectableSourceItem(Data.Source source)
        {
            _source = source;
            Name = source.Name;
            Tags = source.Metadata.Where(x => x.Type == Data.MetadataType.Tag).Select(x => x.Value).ToArray();
            Category = "";
        }

        public bool Selected 
        { 
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public string Category { get; set; }


        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
