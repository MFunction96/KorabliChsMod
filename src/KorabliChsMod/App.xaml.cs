using System.Configuration;
using System.Data;
using System.Windows;

namespace KorabliChsMod
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly MainWindow _mainWindow;

        public App()
        {
            
        }
        
        public App(MainWindow mainWindow)
        {
            this._mainWindow = mainWindow;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            this._mainWindow.Show();
            base.OnStartup(e);
        }
    }

}
