using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TrainingSourceManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Data.DataContext x = new Data.DataContext();
            x.Database.EnsureDeleted();
            x.Database.EnsureCreated();


            Data.Source source = new Data.Source("Test");
            source.AddMetadata(Data.MetadataType.Category, "TestCat");
            source.AddFile(new System.IO.FileInfo("TrainingSourceManager.dll.config"));

            x.Sources.Add(source);
            x.SaveChanges();
        }
    }
}
