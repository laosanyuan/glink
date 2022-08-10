using Glink.Demo.Sdk;
using Glink.Demo.Sdk.Grpc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Glink.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.MouseMove += (sender, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await Task.Delay(5 * 1000);
            var host = DemoHostBuilder.CreateHostBuilder(default).ConfigureServices(t =>
            {
                t.AddSingleton<IViewModelLocator, ViewModelLocator>();
            }).Build();
            host?.RunAsync();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ChangeWindowStatus(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }
    }
}
