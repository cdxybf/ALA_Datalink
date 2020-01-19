using ProgrammingParadigms;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DomainAbstractions
{
    // This is the main window of the application. The output IUI in it takes the responsibility of getting the WPF UIElements 
    // in a hierarchical calling order as it goes deeper in the abstrations wired to it. The output IEvent starts to execute once 
    // the app starts run, which informs the abstraction who implments it to do the things it wants. This abstraction has two inputs:
    // 1. The IEvent for close the window(as well as exit the application);
    // 2. The IDataFlow<bool> to enable(true) or disable(false, grey out) the UI;
    public class MainWindow : IEvent, IDataFlow<bool>
    {
        // outputs
        private IUI iuiStructure;
        private IEvent eventAppStartsRun;

        // private fields
        private Window window;

        /// <summary>
        /// Generates the main UI window of the application and emits a signal that the Application starts running.
        /// </summary>
        /// <param name="title">title of the window</param>
        public MainWindow(string title = null)
        {
            window = new Window()
            {
                Title = title,
                Height = SystemParameters.PrimaryScreenHeight * 0.65,
                Width = SystemParameters.PrimaryScreenWidth * 0.6,
                MinHeight = 500,
                MinWidth = 750,
                Background = Brushes.White,
                Icon = new BitmapImage(new Uri(@"pack://application:,,,/" + Assembly.GetExecutingAssembly().GetName().Name + @";component/Application/Resources/DataLink.ico", UriKind.Absolute)),
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.Loaded += (object sender, RoutedEventArgs e) =>
            {
                eventAppStartsRun?.Execute();
            };
        }

        public void Run()
        {
            window.Content = iuiStructure.GetWPFElement();
            System.Windows.Application app = new System.Windows.Application();
            app.Run(window);
        }

        // IEvent implementation -------------------------------------------------------
        void IEvent.Execute() => System.Windows.Application.Current.Shutdown();

        // IDataFlow<bool> implementation ----------------------------------------------
        bool IDataFlow<bool>.Data { set => window.IsEnabled = value; }
    }
}
