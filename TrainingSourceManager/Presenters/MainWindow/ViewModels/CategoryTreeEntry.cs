using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TrainingSourceManager.Presenters.MainWindow.ViewModels
{
    public class CategoryTreeEntry : ITreeEntry
    {
        public CategoryTreeEntry(IEnumerable<ITreeEntry> subEntries, string caption)
        {
            Entries = subEntries;
            Caption = caption;
        }

        public IEnumerable<ITreeEntry> Entries { get; }

        public string Caption { get; set; }
    }
}
