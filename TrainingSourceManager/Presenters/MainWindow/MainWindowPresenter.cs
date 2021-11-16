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
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _loading;
        private Data.DataContext? _dataContext;
        private string _filter;
        private readonly List<ViewModels.SelectableSourceItem> _sourceItems;
        private List<ViewModels.ITreeEntry> _sourceTreeEntries;

        public bool IsLoading => _loading;

        public SourceDetailsPresenter? SelectedSourceDetails { get; private set; }
        public bool CrossNest { get; set; }
        public string Filter
        {
            set
            {
                _filter = value;

                OnPropertyChanged(nameof(SourceTreeEntries));
            }
        }


        public System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry> SourceTreeEntries
        {
            get
            {
                IQueryable<ViewModels.ITreeEntry> itemQuery = _sourceTreeEntries.AsQueryable();

                if (String.IsNullOrWhiteSpace(_filter) == false)
                {
                    itemQuery = itemQuery.Where(x => x.Caption.Contains(_filter, StringComparison.OrdinalIgnoreCase));
                }

                return new System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry>(itemQuery);
            }
        }



        public string Status { get; private set; }

        public MainWindowPresenter()
        {
            _sourceItems = new List<ViewModels.SelectableSourceItem>();
            _sourceTreeEntries = new List<ViewModels.ITreeEntry>();
            _filter = "";
            Status = "";
        }

        public async Task LoadData()
        {
            if (_loading)
                return;
            _loading = true;

            if (_dataContext != null)
                await _dataContext.DisposeAsync();

            _dataContext = new Data.DataContext();
            await _dataContext.Database.EnsureCreatedAsync();


            List<Data.Source> sources = await _dataContext.Sources.Include(x => x.Metadata).Include(x => x.Files).ToListAsync();

            _sourceItems.Clear();
            _sourceItems.AddRange(sources.Select(x => new ViewModels.SelectableSourceItem(x)));

            _loading = false;
            await Refresh();
        }

        public async Task Refresh()
        {
            if (_loading)
                return;

            _loading = true;
            await Task.Run(() =>
            {
                string[] tags = _sourceItems.SelectMany(x => x.Tags).Distinct().ToArray();

                List<ViewModels.ITreeEntry> list = new List<ViewModels.ITreeEntry>();
                if (CrossNest)
                {
                    System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                    stopwatch.Start();

                    Task[] tasks = new Task[tags.Length];
                    for (int i = 0; i < tags.Length; i++)
                    {
                        string tag = tags[i];
                        tasks[i] = Task.Run(() =>
                        {
                            ViewModels.SelectableSourceItem[] branchSources = _sourceItems.Where(x => x.Tags.Contains(tag)).ToArray();
                            string[] branchTags = branchSources.SelectMany(x => x.Tags).Distinct().Where(x => x != tag).ToArray();
                            list.Add(new ViewModels.CategoryTreeEntry(NestTag(branchTags, branchSources), tag));
                        });
                    }
                    Task.WaitAll(tasks);

                    stopwatch.Stop();
                    System.Diagnostics.Debug.WriteLine("Cross-nest: " + stopwatch.ElapsedMilliseconds + "ms");
                    GC.Collect();
                }
                else
                {
                    foreach (string tag in tags)
                        list.Add(new ViewModels.CategoryTreeEntry(_sourceItems.Where(x => x.Tags.Contains(tag)).Select(x => new ViewModels.SourceTreeEntry(x)), tag));
                }

                list = list.OrderBy(x => x.Caption).ToList();
                list.AddRange(_sourceItems.Where(x => x.Tags.Length == 0).Select(x => new ViewModels.SourceTreeEntry(x)));

                _sourceTreeEntries = list;

            });


            OnPropertyChanged(nameof(SourceTreeEntries));
            UpdateStatus();
            _loading = false;
        }



        private List<ViewModels.ITreeEntry> NestTag(string[] remainingTags, ViewModels.SelectableSourceItem[] sources)
        {
            var result = new List<ViewModels.ITreeEntry>();
            List<ViewModels.SelectableSourceItem> remaining = sources.ToList();

            for (int i = 0; i < remainingTags.Length; i++)
            {
                ViewModels.SelectableSourceItem[] taggedSources = sources.Where(x => x.Tags.Contains(remainingTags[i])).ToArray();
                if (taggedSources.Length > 0)
                {
                    remaining.RemoveAll(x => taggedSources.Contains(x));
                    result.Add(new ViewModels.CategoryTreeEntry(NestTag(remainingTags.Where(x => x != remainingTags[i]).ToArray(), taggedSources), remainingTags[i]));
                }
            }

            if (remaining.Count > 0)
                result.AddRange(remaining.Select(x => new ViewModels.SourceTreeEntry(x)));

            return result;
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

        public void UpdateStatus()
        {
            List<Data.Source> sources = new List<Data.Source>();
            GetSelected(SourceTreeEntries, ref sources);
            Status = $"{sources.Count} ({(sources.SelectMany(x => x.Files.Select(y => y.Length)).Sum() / 1024D):N1} Kb)";

            OnPropertyChanged(nameof(Status));
        }
    }
}
