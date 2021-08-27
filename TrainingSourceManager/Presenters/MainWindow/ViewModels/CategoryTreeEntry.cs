using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.MainWindow.ViewModels
{
    public class CategoryTreeEntry: ITreeEntry
    {
        public CategoryTreeEntry(IEnumerable<ITreeEntry> subEntries, string name)
        {
            Entries = subEntries;
            Name = name;
        }

        public IEnumerable<ITreeEntry> Entries { get; }

        public string Name { get; set; }
    }
}
