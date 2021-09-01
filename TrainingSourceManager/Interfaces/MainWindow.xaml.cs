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
            RefreshData();
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
                            RefreshData();
                    }));
                }
            }
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

        private async void Selected_Export(object sender, RoutedEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.AutoUpgradeEnabled = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    await Presenter.ExportSelectedSources(dialog.SelectedPath);
            }
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
                RefreshData();
        }

        private async void RefreshData()
        {
            SourceTree.IsEnabled = false;
            await Presenter.LoadData();
            SourceTree.IsEnabled = true;
        }

        private void Edit_Delete(object sender, RoutedEventArgs e)
        {
            DeleteSelected();
        }

        private async void DeleteSelected()
        {
            if (SourceTree.SelectedItem is SourceTreeEntry entry)
            {
                if (MessageBox.Show($"Are you sure you wish to delete '{entry.Caption}'") == MessageBoxResult.Yes)
                    await Presenter.DeleteSource(entry);
            }
        }

        private async void Edit_Backup(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog
            {
                Filter = "Backup (*.bak)|*.bak"
            };
            if (fileDialog.ShowDialog() == true)
                await Presenter.BackupData(fileDialog.FileName);
        }

        private async void Edit_Restore(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Filter = "Backup (*.bak)|*.bak"
            };
            if (fileDialog.ShowDialog() == true)
                await Presenter.RestoreData(fileDialog.FileName);
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

            LoadDetails(save);
        }

        private void SourceDetail_Save(object sender, RoutedEventArgs e)
        {
            LoadDetails(true);
        }

        private void SourceDetail_Cancel(object sender, RoutedEventArgs e)
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (Presenter.SelectedSourceDetails.HasChanges)
            {
                LoadDetails(false);
            }
        }

        private async void LoadDetails(bool saveChanges)
        {
            SourceDetailGrid.IsEnabled = false;
            await Presenter.SelectSource(SourceTree.SelectedItem as SourceTreeEntry, saveChanges);

            if (Presenter.SelectedSourceDetails != null)
                SourceDetailGrid.IsEnabled = true;
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

        private async void DeleteDetailTag()
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (SourceDetailTagsList.SelectedItem is string tag)
                await Presenter.SelectedSourceDetails.DeleteTag(tag);
        }

        private async void DeleteDetailFile()
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (SourceDetailFileGrid.SelectedItem is FileViewModel fileView)
                await Presenter.SelectedSourceDetails.DeleteFile(fileView);

        }

        private async void SourceDetailFileGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (SourceDetailFileGrid.SelectedItem is FileViewModel fileView)
            {
                System.IO.FileInfo? file = await Presenter.SelectedSourceDetails.ExportFile(fileView, System.IO.Path.GetTempPath());
                if (file != null)
                {
                    var p = new System.Diagnostics.Process()
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo(file.FullName)
                        {
                            UseShellExecute = true
                        }
                    };
                    p.Start();
                }
            }
        }

        private void SourceDetailFileGrid_DragOver(object sender, DragEventArgs e)
        {
            if (e.OriginalSource != sender && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private async void SourceDetailFileGrid_Drop(object sender, DragEventArgs e)
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Handled = true;
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (filePaths.Length > 0)
                    await Presenter.SelectedSourceDetails.AddFiles(filePaths);
            }
        }

        private async void SourceDetailFileGrid_MouseMove(object sender, MouseEventArgs e)
        {
            if (SourceDetailFileGrid.IsEnabled == false)
                return;


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SourceDetailFileGrid.IsEnabled = false;
                if (Presenter.SelectedSourceDetails == null)
                    return;

                List<string> files = new List<string>();
                foreach (FileViewModel fileView in SourceDetailFileGrid.SelectedItems)
                {
                    System.IO.FileInfo? file = await Presenter.SelectedSourceDetails.ExportFile(fileView, System.IO.Path.GetTempPath());
                    if (file != null)
                        files.Add(file.FullName);
                }

                if (files.Count > 0)
                {
                    DataObject dataObject = new DataObject(DataFormats.FileDrop, files.ToArray());
                    DragDrop.DoDragDrop(SourceDetailFileGrid, dataObject, DragDropEffects.Copy);
                }
            }
            SourceDetailFileGrid.IsEnabled = true;
        }

        private async void CrossNest_Click(object sender, RoutedEventArgs e)
        {
            SourceTree.IsEnabled = false;
            await Presenter.Refresh();
            SourceTree.IsEnabled = true;
        }

        private void SourceDetail_TagAdd_Click(object sender, RoutedEventArgs e)
        {
            AddSourceDetailTag();
        }

        private async void AddSourceDetailTag()
        {
            if (Presenter.SelectedSourceDetails == null)
                return;

            string tag = SourceDetail_TagTextbox.Text;
            if (String.IsNullOrWhiteSpace(tag) == false)
            {
                await Presenter.SelectedSourceDetails.AddTag(tag);
                SourceDetail_TagTextbox.Text = null;
            }
        }

        private void SourceDetail_TagRemove_Click(object sender, RoutedEventArgs e)
        {
            DeleteDetailTag();
        }

        private void SourceDetail_TagTextbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddSourceDetailTag();
        }

        private void Reload_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }
    }
}
