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
        private SelectableSourceItem _sourceItem;

        public SourceTreeEntry(SelectableSourceItem sourceItem)
        {
            _sourceItem = sourceItem;
            _sourceItem.PropertyChanged += (s, e) => PropertyChanged?.Invoke(this, e);
        }

        public bool Selected 
        { 
            get => _sourceItem.Selected;
            set => _sourceItem.Selected = value;
        }

        public string Name => _sourceItem.Name;
        public string Tags => String.Join(", ", _sourceItem.Tags);
    }
}
