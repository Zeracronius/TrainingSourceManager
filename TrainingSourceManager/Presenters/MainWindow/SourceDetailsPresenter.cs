using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TrainingSourceManager.Presenters.MainWindow.ViewModels;

namespace TrainingSourceManager.Presenters.MainWindow
{
    public class SourceDetailsPresenter: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly int _sourceId;
        private Data.Source? _source;
        private Data.DataContext? _dataContext;

        public bool HasChanges => _dataContext?.ChangeTracker.HasChanges() ?? false;

        public string? Name
        {
            get => _source?.Name;
            set
            {
                if (_source != null && String.IsNullOrWhiteSpace(value) == false)
                    _source.Name = value;
            }
        }

        public ObservableCollection<FileViewModel> Files { get; private set; }
        public ObservableCollection<string> Tags { get; private set; }

        public SourceDetailsPresenter(Data.Source source)
        {
            _sourceId = source.Id;
            Tags = new ObservableCollection<string>();
            Files = new ObservableCollection<FileViewModel>();
        }


        public async Task LoadData()
        {
            _dataContext?.Dispose();
            _dataContext = new Data.DataContext();
            _source = await _dataContext.Sources.Include(x => x.Files)
                                                .Include(x => x.Metadata)
                                                .FirstOrDefaultAsync(x => x.Id == _sourceId);
            await RefreshCollections();
        }

        public async Task RefreshCollections()
        {
            if (_source == null)
                return;

            await Task.Run(() =>
            {
                Files = new ObservableCollection<FileViewModel>(_source.Files.Select(x => new FileViewModel(x)));
                Tags = new ObservableCollection<string>(_source.Metadata.Where(x => x.Type == Data.MetadataType.Tag).Select(x => x.Value));
            });

            OnPropertyChanged(nameof(Files));
            OnPropertyChanged(nameof(Tags));
        }

        public async Task DeleteTag(string tag)
        {
            if (String.IsNullOrWhiteSpace(tag) || _source == null)
                return;

            if (Tags.Contains(tag))
            {
                Data.Metadata? metaTag = _source.Metadata.FirstOrDefault(x => x.Type == Data.MetadataType.Tag && x.Value.ToLower() == tag.ToLower());
                if (metaTag != null)
                {
                    _source.Metadata.Remove(metaTag);
                    await RefreshCollections();
                }
            }
        }

        public async Task AddTag(string tag)
        {
            if (String.IsNullOrWhiteSpace(tag) || _source == null || _dataContext == null)
                return;

            string[] existingTags = _dataContext.Sources.SelectMany(x => x.Metadata.Where(y => y.Type == Data.MetadataType.Tag).Select(y => y.Value)).Distinct().ToArray();
            string? existingTag = existingTags.FirstOrDefault(x => x.Equals(tag, StringComparison.OrdinalIgnoreCase));
            tag = existingTag ?? tag;

            if (Tags.Contains(tag) == false)
            {
                _source.AddMetadata(Data.MetadataType.Tag, tag);
                await RefreshCollections();
            }
        }

        public async Task DeleteFile(FileViewModel file)
        {
            if (_source == null)
                return;

            _source.Files.Remove(file.File);
            await RefreshCollections();
        }

        public async Task AddFiles(params string[] files)
        {
            if (_source == null)
                return;

            foreach (string file in files)
            {
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(file);
                if (fileInfo.Exists)
                    _source.AddFile(fileInfo);
            }
            await RefreshCollections();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal Task SaveChanges()
        {
            return _dataContext?.SaveChangesAsync() ?? Task.CompletedTask;
        }

        internal async Task<System.IO.FileInfo?> ExportFile(FileViewModel fileView, string directoryPath)
        {
            if (_dataContext == null)
                return null;

            Data.File file = fileView.File;
            if (file.FileData == null)
                await _dataContext.Entry(file).Reference(x => x.FileData).LoadAsync();
            return await file.Export(directoryPath);
        }
    }
}
