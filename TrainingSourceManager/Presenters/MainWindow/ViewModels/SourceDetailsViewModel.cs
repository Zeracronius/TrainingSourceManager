using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.MainWindow.ViewModels
{
    public class SourceDetailsViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        internal Data.Source Source { get; private set; }


        public string Name
        {
            get => Source.Name;
            set => Source.Name = value;
        }

        public ObservableCollection<FileViewModel> Files { get; private set; }
        public ObservableCollection<string> Tags { get; private set; }

        public SourceDetailsViewModel(Data.Source source)
        {
            Source = source;
            Tags = new ObservableCollection<string>();
            Files = new ObservableCollection<FileViewModel>();
        }


        public void LoadData()
        {
            Files = new ObservableCollection<FileViewModel>(Source.Files.Select(x => new FileViewModel(x)));
            Tags = new ObservableCollection<string>(Source.Metadata.Where(x => x.Type == Data.MetadataType.Tag).Select(x => x.Value));

            OnPropertyChanged(nameof(Files));
            OnPropertyChanged(nameof(Tags));
        }

        public void DeleteTag(string tag)
        {
            if (String.IsNullOrWhiteSpace(tag))
                return;

            if (Tags.Contains(tag))
            {
                Data.Metadata? metaTag = Source.Metadata.FirstOrDefault(x => x.Type == Data.MetadataType.Tag && x.Value.ToLower() == tag.ToLower());
                if (metaTag != null)
                {
                    Source.Metadata.Remove(metaTag);
                    LoadData();
                }
            }
        }

        public void AddTag(string tag)
        {
            if (String.IsNullOrWhiteSpace(tag))
                return;

            if (Tags.Contains(tag) == false)
            {
                Source.AddMetadata(Data.MetadataType.Tag, tag);
                LoadData();
            }
        }

        public void DeleteFile(FileViewModel file)
        {
            Source.Files.Remove(file.File);
            LoadData();
        }

        public void AddFile(string path)
        {
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            if (fileInfo.Exists)
            {
                Source.AddFile(fileInfo);
                LoadData();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
