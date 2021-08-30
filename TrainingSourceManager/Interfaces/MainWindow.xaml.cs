using Microsoft.Win32;
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
using TrainingSourceManager.Presenters.MainWindow.ViewModels;

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

        private void ContextMenu_Unselect(object sender, RoutedEventArgs e)
        {
            if (SourceTree.SelectedItem != null)
                SetSelection((ITreeEntry)SourceTree.SelectedItem, false);
        }

        private void ContextMenu_Select(object sender, RoutedEventArgs e)
        {
            if (SourceTree.SelectedItem != null)
                SetSelection((ITreeEntry)SourceTree.SelectedItem, true);
        }

        private void SetSelection(ITreeEntry item, bool selected)
        {
            switch (item)
            {
                case CategoryTreeEntry category:
                    foreach (ITreeEntry entry in category.Entries)
                        SetSelection(entry, selected);
                    break;

                case SourceTreeEntry source:
                    source.Selected = selected;
                    break;
            }
        }

        private void Selected_Export(object sender, RoutedEventArgs e)
        {
            
        }

        private void Selected_Clear(object sender, RoutedEventArgs e)
        {
            foreach (ITreeEntry item in SourceTree.ItemsSource)
                SetSelection(item, false);
        }

        private void Selected_SelectAll(object sender, RoutedEventArgs e)
        {
            foreach (ITreeEntry item in SourceTree.ItemsSource)
                SetSelection(item, true);
        }

        private void Edit_Add(object sender, RoutedEventArgs e)
        {
            Presenters.AddSource.AddSourcePresenter addPresenter = new Presenters.AddSource.AddSourcePresenter();
            AddSource addDialog = new AddSource(addPresenter);
            if (addDialog.ShowDialog() == true)
                Presenter.LoadData();
        }

        private void Edit_Delete(object sender, RoutedEventArgs e)
        {
            DeleteSelected();
        }

        private void DeleteSelected()
        {
            if (SourceTree.SelectedItem is SourceTreeEntry entry)
            {
                if (MessageBox.Show($"Are you sure you wish to delete '{entry.Caption}'") == MessageBoxResult.Yes)
                    Presenter.DeleteSource(entry);
            }
        }

        private void Edit_Backup(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "Backup (*.bak)|*.bak";
            if (fileDialog.ShowDialog() == true)
            {
                Presenter.BackupData(fileDialog.FileName);
            }
        }

        private void Edit_Restore(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Backup (*.bak)|*.bak";
            if (fileDialog.ShowDialog() == true)
            {
                Presenter.RestoreData(fileDialog.FileName);
            }
        }
        private void SourceTree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem? treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();

                switch (SourceTree.SelectedItem)
                {
                    case SourceTreeEntry _:
                        SourceTree.ContextMenu = SourceTree.Resources["SourceItemContextMenu"] as ContextMenu;
                        break;

                    case CategoryTreeEntry _:
                        SourceTree.ContextMenu = SourceTree.Resources["CategoryContextMenu"] as ContextMenu;
                        break;
                }
            }
            else
            {
                SourceTree.ContextMenu = null;
            }

            e.Handled = true;
        }

        static TreeViewItem? VisualUpwardSearch(DependencyObject? source)
        {
            while (source != null && (source is TreeViewItem) == false)
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }
        private void ContextMenu_Delete(object sender, RoutedEventArgs e)
        {
            DeleteSelected();
        }

        private void SourceTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            bool save = false;
            if (Presenter.SelectedSourceDetails?.HasChanges ?? false)
            {
                MessageBoxResult saveChanges = MessageBox.Show("You have unsaved changes to the currently selected source. Do you wish to save them?", "Unsaved changes", MessageBoxButton.YesNo);
                if (saveChanges == MessageBoxResult.Yes)
                    save = true;
            }

            Presenter.SelectSource(SourceTree.SelectedItem as SourceTreeEntry, save);
        }

        private void SourceDetail_Save(object sender, RoutedEventArgs e)
        {
            Presenter.SelectSource(SourceTree.SelectedItem as SourceTreeEntry, true);
        }

        private void SourceDetail_Cancel(object sender, RoutedEventArgs e)
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (Presenter.SelectedSourceDetails.HasChanges)
            {
                Presenter.SelectSource(SourceTree.SelectedItem as SourceTreeEntry, false);
            }
        }

        private void SourceDetail_FileGrid_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                DeleteDetailFile();
        }

        private void SourceDetail_TagList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
                DeleteDetailTag();
        }

        private void DeleteDetailTag()
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (SourceDetailTagsList.SelectedItem is string tag)
                Presenter.SelectedSourceDetails.DeleteTag(tag);
        }

        private void DeleteDetailFile()
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (SourceDetailFileGrid.SelectedItem is FileViewModel fileView)
                Presenter.SelectedSourceDetails.DeleteFile(fileView);

        }

        private void SourceDetailFileGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (SourceDetailFileGrid.SelectedItem is FileViewModel fileView)
            {
                System.IO.FileInfo? file = Presenter.SelectedSourceDetails.ExportFile(fileView, System.IO.Path.GetTempPath());
                if (file != null)
                {
                    var p = new System.Diagnostics.Process();
                    p.StartInfo = new System.Diagnostics.ProcessStartInfo(file.FullName)
                    {
                        UseShellExecute = true
                    };
                    p.Start();
                }
            }
        }

        private void SourceDetailFileGrid_DragOver(object sender, DragEventArgs e)
        {

        }

        private void SourceDetailFileGrid_Drop(object sender, DragEventArgs e)
        {

        }

        private void SourceDetailFileGrid_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {

        }

        private void SourceDetailFileGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                List<string> files = new List<string>();
                foreach (FileViewModel fileView in SourceDetailFileGrid.SelectedItems)
                {
                    System.IO.FileInfo? file = Presenter.SelectedSourceDetails.ExportFile(fileView, System.IO.Path.GetTempPath());
                    if (file != null)
                        files.Add(file.FullName);
                }

                if (files.Count > 0)
                {
                    DataObject dataObject = new DataObject(DataFormats.FileDrop, files.ToArray());
                    DragDrop.DoDragDrop(SourceDetailFileGrid, dataObject, DragDropEffects.Copy);
                }
            }
        }
    }
}
