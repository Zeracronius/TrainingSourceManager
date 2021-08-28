﻿using System;
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

        private void FileList_Drop(object sender, DragEventArgs e)
        {
            string[] filePaths = (string[])e.Data.GetData("FileNameW");
            if (filePaths.Length > 0)
                Presenter.AddFiles(filePaths);
            e.Handled = true;
        }

        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            Presenter.AddTag(AddTagTextbox.Text);
            AddTagTextbox.Text = string.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

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
    }
}