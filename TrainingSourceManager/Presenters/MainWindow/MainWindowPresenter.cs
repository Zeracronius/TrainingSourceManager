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
        private Data.DataContext? _dataContext;
        private List<ViewModels.SelectableSourceItem> _sourceItems;

        public event PropertyChangedEventHandler? PropertyChanged;

        public System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry> SourceTreeEntries { get; set; }

        public SourceDetailsPresenter? SelectedSourceDetails { get; private set; }


        public bool CrossNest { get; set; }

        public MainWindowPresenter()
        {
            SourceTreeEntries = new System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry>();
            _sourceItems = new List<ViewModels.SelectableSourceItem>();
        }

        public async Task LoadData()
        {
            if (_dataContext != null)
                await _dataContext.DisposeAsync();

            _dataContext = new Data.DataContext();
            await _dataContext.Database.EnsureCreatedAsync();


            List<Data.Source> sources = await _dataContext.Sources.Include(x => x.Metadata).ToListAsync();

            _sourceItems.Clear();
            _sourceItems.AddRange(sources.Select(x => new ViewModels.SelectableSourceItem(x)));
            await Refresh();
        }

        public async Task Refresh()
        {

            await Task.Run(() =>
            {
                IEnumerable<string> tags = _sourceItems.SelectMany(x => x.Tags).Distinct();

                List<ViewModels.ITreeEntry> list = new List<ViewModels.ITreeEntry>();
                if (CrossNest)
                {
                    HashSet<string> includedTags = new HashSet<string>();
                    foreach (string tag in tags)
                    {
                        includedTags.Clear();
                        list.Add(new ViewModels.CategoryTreeEntry(NestTag(includedTags, tag, _sourceItems.Where(x => x.Tags.Contains(tag))), tag));
                    }
                }
                else
                {
                    foreach (string tag in tags)
                        list.Add(new ViewModels.CategoryTreeEntry(_sourceItems.Where(x => x.Tags.Contains(tag)).Select(x => new ViewModels.SourceTreeEntry(x)), tag));
                }

                list = list.OrderBy(x => x.Caption).ToList();
                list.AddRange(_sourceItems.Where(x => x.Tags.Length == 0).Select(x => new ViewModels.SourceTreeEntry(x)));

                SourceTreeEntries = new System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry>(list);

            });
            OnPropertyChanged(nameof(SourceTreeEntries));
        }



        private List<ViewModels.ITreeEntry> NestTag(HashSet<string> includedTags, string currentTag, IEnumerable<ViewModels.SelectableSourceItem> sources)
        {
            includedTags.Add(currentTag);
            IEnumerable<string> tags = sources.SelectMany(x => x.Tags).Distinct();
            IEnumerable<string> unhandledTags = tags.Where(x => includedTags.Contains(x) == false);
            if (unhandledTags.Any())
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


        public async Task BackupData(string filepath)
        {
            if (_dataContext == null)
                return;

            await _dataContext.Database.ExecuteSqlRawAsync($"BACKUP DATABASE [TrainingSourceManager] TO DISK = '{filepath}'");
        }

        public async Task RestoreData(string filepath)
        {
            if (_dataContext == null)
                return;

            await _dataContext.Database.ExecuteSqlRawAsync($"RESTORE DATABASE [TrainingSourceManager] FROM DISK = '{filepath}' WITH REPLACE");
            await LoadData();
        }

        public async Task DeleteSource(ViewModels.SourceTreeEntry source)
        {
            using (var context = new Data.DataContext())
            {
                context.Sources.Remove(source.Source);
                await context.SaveChangesAsync();
            }
            await LoadData();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task SelectSource(ViewModels.SourceTreeEntry? sourceEntry, bool saveChanges)
        {
            if (saveChanges && SelectedSourceDetails != null)
                await SelectedSourceDetails.SaveChanges();

            if (sourceEntry != null)
            {
                SelectedSourceDetails = new SourceDetailsPresenter(sourceEntry.Source);
                await SelectedSourceDetails.LoadData();
            }
            else
                SelectedSourceDetails = null;

            OnPropertyChanged(nameof(SelectedSourceDetails));
        }





        public Task ExportSelectedSources(string directoryPath)
        {
            if (_dataContext == null)
                return Task.CompletedTask;

            return Task.Run(() =>
            {
                List<Data.Source> selected = new List<Data.Source>();
                GetSelected(SourceTreeEntries, ref selected);

                
                List<Task> exportTasks = new List<Task>();
                foreach (Data.Source source in selected)
                {
                    _dataContext.Entry(source).Collection(x => x.Files).Query().Include(x => x.FileData).Load();
                    foreach (Data.File file in source.Files)
                        exportTasks.Add(file.Export(directoryPath));
                }

                Task.WaitAll(exportTasks.ToArray());
            });
        }

        
        private void GetSelected(IEnumerable<ViewModels.ITreeEntry> items, ref List<Data.Source> selected)
        {
            foreach (ViewModels.ITreeEntry entry in items)
            {
                switch (entry)
                {
                    case ViewModels.CategoryTreeEntry category:
                            GetSelected(category.Entries, ref selected);
                        break;

                    case ViewModels.SourceTreeEntry source:
                        if (source.Selected && selected.Contains(source.Source) == false)
                            selected.Add(source.Source);
                        break;
                }
            }
        }
    }
}
