using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.MainWindow.ViewModels
{
    public class SourceTreeEntry: ITreeEntry, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly SelectableSourceItem _sourceItem;

        public SourceTreeEntry(SelectableSourceItem sourceItem)
        {
            _sourceItem = sourceItem;
        }

        public bool Selected 
        { 
            get => _sourceItem.Selected;
            set
            {
                _sourceItem.Selected = value;
                OnPropertyChanged();
            }
        }

        public string Name => _sourceItem.Name;
        public string Tags => _sourceItem.Tags;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
