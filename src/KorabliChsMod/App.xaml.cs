using System.Windows;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Xanadu.KorabliChsMod
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
