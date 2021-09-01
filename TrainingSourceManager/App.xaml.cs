using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace TrainingSourceManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Manager.TempFileManager.Clean();
            Interfaces.MainWindow window = new Interfaces.MainWindow(new Presenters.MainWindow.MainWindowPresenter());
            window.Show();
        }
    }
}
