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
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void SourceTree_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Handled = true;
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (filePaths.Length > 0)
                {
                    System.Threading.Thread.Yield();
                    Presenters.AddSource.AddSourcePresenter addPresenter = new Presenters.AddSource.AddSourcePresenter(filePaths);
                    AddSource addDialog = new AddSource(addPresenter);
                    this.Dispatcher.BeginInvoke((Action)(() => 
                    {
                        if (addDialog.ShowDialog() == true)
                            Presenter.LoadData();
                    }));
                }
            }
        }

        private void SourceTree_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            if (e.Effects.HasFlag(DragDropEffects.Copy))
                Mouse.SetCursor(Cursors.Cross);
            else
                Mouse.SetCursor(Cursors.No);

            e.Handled = true;
        }
    }
}
