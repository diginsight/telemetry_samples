#region using
using Common;
using EasySample600v2;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using Metrics = System.Collections.Generic.Dictionary<string, object>; // $$$
#endregion

namespace EasySample
{
    //public class C : WeakEventManager { }
    /// <summary>Interaction logic for MainWindow.xaml</summary>
    public partial class MainWindow : Window
    {
        static Type T = typeof(MainWindow);
        private ILogger<MainWindow> logger;
        private IClassConfigurationGetter<MainWindow> classConfigurationGetter;

        private string GetScope([CallerMemberName] string memberName = "") { return memberName; }

        public IList<ImageSource> Images
        {
            get { return (IList<ImageSource>)GetValue(ImagesProperty); }
            set { SetValue(ImagesProperty, value); }
        }
        public static readonly DependencyProperty ImagesProperty = DependencyProperty.Register("Images", typeof(IList<ImageSource>), typeof(MainWindow), new PropertyMetadata());

        static MainWindow()
        {
            var host = App.Host;
            using var scope = host.BeginMethodScope<MainWindow>();
        }
        public MainWindow(
            ILogger<MainWindow> logger,
            IClassConfigurationGetter<MainWindow> classConfigurationGetter
            )
        {
            this.logger = logger;
            this.classConfigurationGetter = classConfigurationGetter;
            using (logger.BeginScope(TraceLogger.GetMethodName()))
            {
                InitializeComponent();
            }
        }

        private async void MainWindow_Initialized(object sender, EventArgs e)
        {
            using var scope = logger.BeginMethodScope(() => new { sender, e });

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using var scope = logger.BeginMethodScope(() => new { sender, e });

            string currentFolder = System.IO.Directory.GetCurrentDirectory();
            string[] files = System.IO.Directory.GetFiles("Images1");

            var images = new List<BitmapSource>();
            foreach (var file in files)
            {
                var filePath = $"{currentFolder}\\{file}".Replace("\\", "/");
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(filePath);
                bitmapImage.EndInit();

                images.Add(bitmapImage);

            }
            Images = images.ToArray();
        }
    }
}
