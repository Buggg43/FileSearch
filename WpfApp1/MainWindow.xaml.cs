using Search.Domain;
using Search.services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Uri Path;
        private string _searchText = "";
        private DispatcherTimer _dispatcherTimer;
        private readonly FilteringService _filteringService;
        private int _searchVersion;

        public ObservableCollection<IndexedFile> Results { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public BitmapImage PreviewSource { get; set; } = null;
        public string PreviewToolTip { get; set; } = "";
        public MainWindow(FilteringService filteringService)
        {
            InitializeComponent();
            DataContext = this;
            Results = new ObservableCollection<IndexedFile>();
            _dispatcherTimer = new DispatcherTimer();
            _filteringService = filteringService;

            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(300);
            _dispatcherTimer.Tick += OnTimedEvent;
        }
        //Actions
        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = (ListBoxItem)sender;
            var file = item.DataContext as IndexedFile;
            if (file == null) return;

            Process.Start(new ProcessStartInfo
            {
                FileName = file.FullPath,
                UseShellExecute = true
            });
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _dispatcherTimer.Stop();
            _searchText = SearchBox.Text;
            _dispatcherTimer.Start();
        }
        private void Window_GotFocus(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }
        //Methods
        /*private void OpenFile(IndexedFile file)
        {
            var process = new ProcessStartInfo()
            {
                FileName = file.FullPath,
                UseShellExecute = true
            };
            Process.Start(process);
        }
        private void OpenFileFolder(IndexedFile file)
        {
            var process = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{file.FullPath}\"",
                UseShellExecute = true
            };
            Process.Start(process);
        }*/
        private async Task FilteredFiles(FilteringService filteringService, string searchQuery, int Version)
        {
            var result = await filteringService.FilterResults(searchQuery);
            if (Version == _searchVersion)
            {
                Results.Clear();
                foreach (var r in result.Take(200))
                {
                    Results.Add(r);
                }
            }
        }
        private async void OnTimedEvent(object? sender, EventArgs e)
        {
            _dispatcherTimer.Stop();
            _searchVersion++;
            var query = _searchText;
            var currentSearch = _searchVersion;

            await FilteredFiles(_filteringService, query, currentSearch);
        }
    }
}