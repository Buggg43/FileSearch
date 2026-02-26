using Microsoft.Extensions.DependencyInjection;
using Search.Domain;
using Search.services;
using System.Windows;
using WpfApp1.servives;


namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ServiceProvider Services { get; private set; }
        public App()
        {
            var services = new ServiceCollection();
            ConfigureService(services);
            Services = services.BuildServiceProvider();
        }
        private void ConfigureService(IServiceCollection services)
        {
            services.AddSingleton<FileParserService>();
            services.AddSingleton<SearchService>();
            services.AddSingleton<IndexService>();
            services.AddSingleton<FileCache>();
            services.AddSingleton<MainWindow>();
        }
        private void ShowMainWindow()
        {
            var mainWindow = Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var indexService = Services.GetRequiredService<IndexService>();
            var fileCacheService = Services.GetRequiredService<FileCache>();

            var cache = await indexService.LoadOrBuildAsync();
            if (cache is null)
            {
                cache = await indexService.BuildIndexAsync();
            }

            fileCacheService.FileIndex = cache.FileIndex;
            fileCacheService.IndexBuildAt = cache.IndexBuildAt;

            ShowMainWindow();
        }
    }

}
