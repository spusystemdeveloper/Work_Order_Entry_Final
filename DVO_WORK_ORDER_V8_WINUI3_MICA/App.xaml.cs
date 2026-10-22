using Microsoft.UI.Xaml;
namespace DVO_STORE_DB
{
    public partial class App : Application
    {
        public App() { this.InitializeComponent(); }
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
        private Window m_window;
    }
}
