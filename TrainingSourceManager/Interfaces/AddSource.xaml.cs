using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using TrainingSourceManager.Presenters.AddSource;

namespace TrainingSourceManager.Interfaces
{
    /// <summary>
    /// Interaction logic for AddSource.xaml
    /// </summary>
    public partial class AddSource : Window
    {
        public AddSourcePresenter Presenter { get; private set; }

        public AddSource(AddSourcePresenter presenter)
        {
            Presenter = presenter;
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void FileList_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            base.OnGiveFeedback(e);
            if (e.Effects.HasFlag(DragDropEffects.Copy))
                Mouse.SetCursor(Cursors.Cross);
            else
                Mouse.SetCursor(Cursors.No);

            e.Handled = true;
        }

        private void FileList_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void FileList_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                Presenter.AddFiles(filePaths);
            }
            e.Handled = true;
        }

        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            CreateTag();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Presenter.SaveChanges();
            DialogResult = true;
            this.Close();
        }

        private void TagsList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Presenter.DeleteTags(TagsList.SelectedItems.Cast<string>().ToArray());
            }
        }

        private void FileList_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                Presenter.DeleteFiles(FileList.SelectedItems.Cast<string>().ToArray());
            }
        }

        private void AddTagTextbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && String.IsNullOrWhiteSpace(AddTagTextbox.Text) == false)
                CreateTag();
        }

        private void CreateTag()
        {
            Presenter.AddTag(AddTagTextbox.Text);
            AddTagTextbox.Text = string.Empty;
        }
    }
}
