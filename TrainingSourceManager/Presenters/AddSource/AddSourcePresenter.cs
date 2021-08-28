using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.AddSource
{
    public class AddSourcePresenter : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; set; }
        public ObservableCollection<string> Tags { get; set; }
        public ObservableCollection<string> Files { get; set; }




        string? IDataErrorInfo.Error => null;

        string? IDataErrorInfo.this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                            return "Name cannot be empty.";
                        break;
                }

                return string.Empty;
            }
        }



        public AddSourcePresenter(params string[] files)
        {
            Files = new ObservableCollection<string>(files);
            Tags = new ObservableCollection<string>();
            Name = string.Empty;

            if (files.Length > 0)
                Name = System.IO.Path.GetFileName(files[0]);
        }


        public void AddFiles(params string[] filesToAdd)
        {
            foreach (string file in filesToAdd)
            {
                if (Files.Any(x => x.ToLower() == file.ToLower()) == false)
                {
                    if (System.IO.File.Exists(file))
                        Files.Add(file);
                }
            }
            OnPropertyChanged(nameof(Files));
        }

        public void DeleteFiles(params string[] filesToDelete)
        {
            foreach (string file in filesToDelete)
            {
                string? existingFile = Files.FirstOrDefault(x => x.ToLower() == file.ToLower());
                if (existingFile != null)
                    Files.Remove(existingFile);
            }
            OnPropertyChanged(nameof(Files));
        }

        public void AddTag(string tag)
        {
            if (String.IsNullOrWhiteSpace(tag))
                return;

            if (Tags.Any(x => x.ToLower() == tag.ToLower()) == false)
            {
                Tags.Add(tag);
                OnPropertyChanged(nameof(Tags));
            }
        }


        public void DeleteTags(params string[] tagsToDelete)
        {
            foreach (string tag in tagsToDelete)
            {
                string? existingTag = Tags.FirstOrDefault(x => x.ToLower() == tag.ToLower());
                if (existingTag != null)
                    Tags.Remove(existingTag);
            }
            OnPropertyChanged(nameof(Tags));
        }

        public void SaveChanges()
        {
            using (Data.DataContext context = new Data.DataContext())
            {
                Data.Source source = new Data.Source(Name);
                foreach (string path in Files)
                {
                    var file = new System.IO.FileInfo(path);
                    if (file.Exists)
                        source.AddFile(file);
                }

                foreach (string tag in Tags)
                    source.AddMetadata(Data.MetadataType.Tag, tag);

                context.Sources.Add(source);
                context.SaveChanges();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
