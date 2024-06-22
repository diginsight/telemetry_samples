using Diginsight.Diagnostics;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ILogger<MainWindow> logger;

        static MainWindow()
        {
            var logger = App.DeferredLoggerFactory.CreateLogger<MainWindow>();
            using (var activity = Observability.ActivitySource.StartMethodActivity(logger))
            {

            }

        }
        public MainWindow()
        {

        }
        public MainWindow(ILogger<MainWindow> logger)
        {
            this.logger = logger;
            using (var activity = Observability.ActivitySource.StartMethodActivity(logger))
            {
                InitializeComponent();
            }
        }
    }
    internal static class Observability
    {
        public static readonly ActivitySource ActivitySource = new ActivitySource(Assembly.GetExecutingAssembly().GetName().Name);
    }
}
