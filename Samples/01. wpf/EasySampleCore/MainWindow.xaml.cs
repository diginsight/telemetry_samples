﻿#region using
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace EasySampleCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Initialized(object sender, EventArgs e)
        {
            using (var sec = this.GetCodeSection(new { sender = sender.GetLogString(), e = e.GetLogString() }))
            {
                sec.Debug("this is a debug trace", "User", properties: new Dictionary<string, object>() { { "", "" } });
                sec.Information("this is a Information trace", "Raw");
                sec.Warning("this is a Warning trace", "User.Report");
                sec.Error("this is a error trace", "Resource");
            }
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            using (var sec = this.GetCodeSection(new { sender = sender.GetLogString(), e = e.GetLogString() }))
            {
                try
                {
                    sec.Debug("this is a debug trace", "User", new Metrics() {
                        { "User", 123 },
                        { "Tags", new[] { "sample", "user", "advanced" } }
                    });

                    sec.Information("this is a Information trace", "event");
                    sec.Information("this is a Information trace", "Raw");
                    sec.Warning("this is a Warning trace", "User.Report");
                    sec.Error("this is a error trace", "Resource");


                    throw new NullReferenceException();
                }
                catch (Exception ex)
                {
                    sec.Exception(ex);
                }


                // report button 
                // var recorder = Trace.Listeners.OfType<TraceListener>().FirstOrDefault(l => l is RecorderTraceListener) as RecorderTraceListener;
                // var entries = recorder.GetItems();
            }
        }

        public void SampleMethod()
        {
            using (var sec = this.GetCodeSection())
            {
                Thread.Sleep(100);
                SampleMethodNested();
                SampleMethodNested1();

            }
        }
        public void SampleMethodNested()
        {
            Thread.Sleep(100);
        }
        public void SampleMethodNested1()
        {
            Thread.Sleep(10);
        }

    }
}
