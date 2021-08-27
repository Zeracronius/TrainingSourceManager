using Microsoft.EntityFrameworkCore;
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
        private bool _crossNest;

        public event PropertyChangedEventHandler? PropertyChanged;

        public System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry> SourceTreeEntries { get; set; }
        public bool CrossNest
        {
            get => _crossNest;
            set
            {
                _crossNest = value;
                LoadData();
            }
        }

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
            IEnumerable<Data.Source> sources = _dataContext.Sources.Include(x => x.Metadata).ToList();
            sources.First().AddMetadata(Data.MetadataType.Tag, "Tag1");
            sources.First().AddMetadata(Data.MetadataType.Tag, "Tag2");
            sources.First().AddMetadata(Data.MetadataType.Tag, "Tag3");
            IEnumerable<ViewModels.SelectableSourceItem> sourceItems = sources.Select(x => new ViewModels.SelectableSourceItem(x));
            List<ViewModels.ITreeEntry> list = new List<ViewModels.ITreeEntry>();

            if (CrossNest)
            {
                IEnumerable<string> tags = sources.SelectMany(x => x.Metadata.Where(x => x.Type == Data.MetadataType.Tag).Select(x => x.Value)).Distinct();

                foreach (string tag in tags)
                {
                    HashSet<string> includedTags = new HashSet<string>();
                    list.Add(new ViewModels.CategoryTreeEntry(NestTag(includedTags, tag, sourceItems.Where(x => x.Tags.Contains(tag))), tag));
                }
            }
            else
            {
                IEnumerable<string> tags = sources.SelectMany(x => x.Metadata.Where(x => x.Type == Data.MetadataType.Tag).Select(x => x.Value)).Distinct();

                foreach (string tag in tags)
                    list.Add(new ViewModels.CategoryTreeEntry(sourceItems.Where(x => x.Tags.Contains(tag)).Select(x => new ViewModels.SourceTreeEntry(x)), tag));
            }

            list.AddRange(sourceItems.Where(x => x.Tags.Length == 0).Select(x => new ViewModels.SourceTreeEntry(x)));


            //IEnumerable<ViewModels.SelectableSourceItem> sourceItems = _dataContext.Sources.ToList().Select(x => new ViewModels.SelectableSourceItem(x));


            //foreach (ViewModels.SelectableSourceItem sourceItem in sourceItems)
            //{
                
            //}


            //List<ViewModels.ITreeEntry> list = new List<ViewModels.ITreeEntry>(sourceItems.Select(x => new ViewModels.SourceTreeEntry(x)));


            //list.AddRange(list);
            //list.AddRange(list);
            //list.Add(new ViewModels.CategoryTreeEntry(sourceItems.Select(x => new ViewModels.SourceTreeEntry(x)), "Category"));
            //list.AddRange(list);


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



        private List<ViewModels.ITreeEntry> NestTag(HashSet<string> includedTags, string currentTag, IEnumerable<ViewModels.SelectableSourceItem> sources)
        {
            includedTags.Add(currentTag);
            IEnumerable<string> tags = sources.SelectMany(x => x.Tags).Distinct();
            IEnumerable<string> unhandledTags = tags.Where(x => includedTags.Contains(x) == false);
            if (unhandledTags.Count() > 0)
            {
                // Not reached bottom yet!
                var result = new List<ViewModels.ITreeEntry>();
                foreach (string tag in unhandledTags)
                    result.Add(new ViewModels.CategoryTreeEntry(NestTag(new HashSet<string>(includedTags), tag, sources.Where(x => x.Tags.Contains(tag))), tag));
                return result;
            }
            else
            {
                return new List<ViewModels.ITreeEntry>(sources.Select(x => new ViewModels.SourceTreeEntry(x)));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
