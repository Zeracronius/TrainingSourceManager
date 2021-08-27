using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Presenters.MainWindow.ViewModels
{
    public class SelectableSourceItem
    {
        readonly Data.Source _source;

        public SelectableSourceItem(Data.Source source)
        {
            _source = source;
            Name = source.Name;
            Tags = source.Metadata.Where(x => x.Type == Data.MetadataType.Tag).Select(x => x.Value).ToArray();
            Category = "";
        }

        public bool Selected { get; set; }
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public string Category { get; set; }
    }
}
