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
using TrainingSourceManager.Presenters.MainWindow;

namespace TrainingSourceManager.Interfaces
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindowPresenter Presenter { get; private set; }

        public MainWindow(MainWindowPresenter presenter)
        {
            Presenter = presenter;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Presenter.LoadData();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var x = SourceTree.Items;
        }

        private void SourceTree_DragOver(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData("FileNameW");
            if (filePaths.Length > 0)
            {
                foreach (string filePath in filePaths)
                {
                    if (System.IO.File.Exists(filePath) == false)
                    {
                        e.Effects = DragDropEffects.None;
                        e.Handled = true;
                        return;
                    }

                    e.Effects = DragDropEffects.Copy;
                    e.Handled = true;
                    return;
                }
            }
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void SourceTree_Drop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData("FileNameW");
            if (filePaths.Length > 0)
            {
                foreach (string filePath in filePaths)
                {
                }
            }
        }

        private void SourceTree_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

            base.OnGiveFeedback(e);
            // These Effects values are set in the drop target's
            // DragOver event handler.
            if (e.Effects.HasFlag(DragDropEffects.Copy))
                Mouse.SetCursor(Cursors.Cross);
            else
                Mouse.SetCursor(Cursors.No);

            e.Handled = true;
        }
    }
}
