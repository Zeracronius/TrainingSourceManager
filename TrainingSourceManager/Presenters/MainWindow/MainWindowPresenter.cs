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

        public bool IsLoading => _loading;

        public SourceDetailsPresenter? SelectedSourceDetails { get; private set; }
        public bool CrossNest { get; set; }
        public string Filter
        {
            set
            {
                _filter = value;
                Refresh().Wait();
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry> SourceTreeEntries { get; private set; }


        public string Status { get; private set; }

        public MainWindowPresenter()
        {
            _sourceItems = new List<ViewModels.SelectableSourceItem>();
            SourceTreeEntries = new System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry>();
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
                IQueryable<ViewModels.SelectableSourceItem> filteredSources = _sourceItems.AsQueryable();
                if (String.IsNullOrWhiteSpace(_filter) == false)
                {
                    string[] filterParts = System.Text.RegularExpressions.Regex.Split(_filter, @"(?=[-+])");

                    string nameFilter = "";
                    List<string> includedTags = new List<string>();
                    List<string> excludedTags = new List<string>();

                    foreach (string filterPart in filterParts)
                    {
                        if (String.IsNullOrWhiteSpace(filterPart))
                            continue;

                        string part = filterPart.Substring(1, filterPart.Length - 1).Trim();

                        switch (filterPart[0])
                        {
                            case '-':
                                if (filterPart.Length == 1)
                                    continue;

                                excludedTags.Add(part);
                                break;

                            case '+':
                                if (filterPart.Length == 1)
                                    continue;

                                includedTags.Add(part);
                                break;

                            default:
                                nameFilter = part;
                                break;
                        }
                    }

                    if (string.IsNullOrEmpty(nameFilter) == false)
                        filteredSources = filteredSources.Where(x => x.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));

                    foreach (string tag in includedTags)
                        filteredSources = filteredSources.Where(x => x.Tags.Any(x => x.StartsWith(tag, StringComparison.OrdinalIgnoreCase)));

                    foreach (string tag in excludedTags)
                        filteredSources = filteredSources.Where(x => x.Tags.Any(x => x.StartsWith(tag, StringComparison.OrdinalIgnoreCase)) == false);
                }

                string[] tags = filteredSources.SelectMany(x => x.Tags).Distinct().ToArray();

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
                            ViewModels.SelectableSourceItem[] branchSources = filteredSources.Where(x => x.Tags.Contains(tag)).ToArray();
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
                        list.Add(new ViewModels.CategoryTreeEntry(filteredSources.Where(x => x.Tags.Contains(tag)).Select(x => new ViewModels.SourceTreeEntry(x)), tag));
                }

                list = list.OrderBy(x => x.Caption).ToList();
                list.AddRange(filteredSources.Where(x => x.Tags.Length == 0).Select(x => new ViewModels.SourceTreeEntry(x)));

                SourceTreeEntries = new System.Collections.ObjectModel.ObservableCollection<ViewModels.ITreeEntry>(list);
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
                List<Data.Source> selected = _sourceItems.Where(x => x.Selected).Select(x => x.Source).ToList();
                //GetSelected(_sourceItems, ref selected);


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

        public void UpdateStatus()
        {
            List<Data.Source> sources = _sourceItems.Where(x => x.Selected).Select(x => x.Source).ToList();
            double bytes = sources.SelectMany(x => x.Files.Select(y => y.Length)).Sum() / 1024D / 1024D;

            Status = $"{sources.Count} ({bytes:N2} Mb)";

            OnPropertyChanged(nameof(Status));
        }
    }
}
