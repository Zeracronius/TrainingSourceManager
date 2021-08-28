using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.AddSource
{
    public class AddSourcePresenter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [Required]
        public string Name { get; set; }
        public ObservableCollection<string> Tags { get; set; }
        public ObservableCollection<string> Files { get; set; }



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

            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
