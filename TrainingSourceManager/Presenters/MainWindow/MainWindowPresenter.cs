using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.MainWindow
{
    public class MainWindowPresenter : INotifyPropertyChanged
    {
        Data.DataContext? _dataContext;

        public event PropertyChangedEventHandler? PropertyChanged;

        public System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry> SourceTreeEntries { get; set; }

        public MainWindowPresenter()
        {
            SourceTreeEntries = new System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry>();
        }

        public void LoadData()
        {
            if (_dataContext != null)
                _dataContext.Dispose();

            _dataContext = new Data.DataContext();
            _dataContext.Database.EnsureCreated();

            
            //SourceTreeEntries.Clear();
            IEnumerable<ViewModels.SelectableSourceItem> sourceItems = _dataContext.Sources.ToList().Select(x => new ViewModels.SelectableSourceItem(x));
            List<ViewModels.ITreeEntry> list = new List<ViewModels.ITreeEntry>(sourceItems.Select(x => new ViewModels.SourceTreeEntry(x)));


            list.AddRange(list);
            list.AddRange(list);
            list.Add(new ViewModels.CategoryTreeEntry(sourceItems.Select(x => new ViewModels.SourceTreeEntry(x)), "Category"));
            list.AddRange(list);


            //Data.DataContext x = new Data.DataContext();
            //x.Database.EnsureDeleted();
            //x.Database.EnsureCreated();


            //Data.Source source = new Data.Source("Test");
            //source.AddMetadata(Data.MetadataType.Category, "TestCat");
            //source.AddFile(new System.IO.FileInfo("TrainingSourceManager.dll.config"));

            //x.Sources.Add(source);
            //x.SaveChanges();

            SourceTreeEntries = new System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry>(list);
            OnPropertyChanged(nameof(SourceTreeEntries));
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
