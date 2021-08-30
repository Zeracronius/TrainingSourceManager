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


        public void LoadData()
        {
            _dataContext?.Dispose();
            _dataContext = new Data.DataContext();
            _source = _dataContext.Sources.Include(x => x.Files)
                                          .Include(x => x.Metadata)
                                          .FirstOrDefault(x => x.Id == _sourceId);
            RefreshCollections();
        }

        public void RefreshCollections()
        {
            if (_source == null)
                return;

            Files = new ObservableCollection<FileViewModel>(_source.Files.Select(x => new FileViewModel(x)));
            Tags = new ObservableCollection<string>(_source.Metadata.Where(x => x.Type == Data.MetadataType.Tag).Select(x => x.Value));

            OnPropertyChanged(nameof(Files));
            OnPropertyChanged(nameof(Tags));
        }

        public void DeleteTag(string tag)
        {
            if (String.IsNullOrWhiteSpace(tag) || _source == null)
                return;

            if (Tags.Contains(tag))
            {
                Data.Metadata? metaTag = _source.Metadata.FirstOrDefault(x => x.Type == Data.MetadataType.Tag && x.Value.ToLower() == tag.ToLower());
                if (metaTag != null)
                {
                    _source.Metadata.Remove(metaTag);
                    RefreshCollections();
                }
            }
        }

        public void AddTag(string tag)
        {
            if (String.IsNullOrWhiteSpace(tag) || _source == null)
                return;

            if (Tags.Contains(tag) == false)
            {
                _source.AddMetadata(Data.MetadataType.Tag, tag);
                RefreshCollections();
            }
        }

        public void DeleteFile(FileViewModel file)
        {
            if (_source == null)
                return;

            _source.Files.Remove(file.File);
            RefreshCollections();
        }

        public void AddFile(string path)
        {
            if (_source == null)
                return;

            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            if (fileInfo.Exists)
            {
                _source.AddFile(fileInfo);
                RefreshCollections();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void SaveChanges()
        {
            _dataContext?.SaveChanges();
        }

        internal System.IO.FileInfo? ExportFile(FileViewModel fileView, string directoryPath)
        {
            if (_dataContext == null)
                return null;

            Data.File file = fileView.File;
            _dataContext.Entry(file).Reference(x => x.FileData).Load();
            return file.Export(directoryPath);
        }
    }
}
