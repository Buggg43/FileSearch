using Search.Domain;
using Search.services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        //private
        private readonly IOServcie _ioService;
        private string _searchText = "";
        private DispatcherTimer _dispatcherTimer;
        private readonly FilteringService _filteringService;
        private int _searchVersion;
        private IndexedFile userSelectedItem;
        //public
        public Uri Path;
        public ObservableCollection<IndexedFile> Results { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public BitmapImage PreviewSource { get; set; } = null;
        public string PreviewToolTip { get; set; } = "";
        public IndexedFile UserSelectedItem 
        { 
            get
            {
                if (FoundFiles.SelectedItem is IndexedFile selectedFile)
                {
                    return selectedFile;
                }
                return null;
            }
            set
            {
                userSelectedItem = value;
                OnPropertyChanged();
            }
        }

        public MainWindow(FilteringService filteringService, IOServcie iOServcie)
        {
            InitializeComponent();
            DataContext = this;
            Results = new ObservableCollection<IndexedFile>();
            _dispatcherTimer = new DispatcherTimer();
            _filteringService = filteringService;
            _ioService = iOServcie;

            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(300);
            _dispatcherTimer.Tick += OnTimedEvent;
            SearchBox.Focus();

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
        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private async Task FilteredFiles(FilteringService filteringService, string searchQuery, int Version)
        {
            var result = await filteringService.FilterResults(searchQuery);
            if(searchQuery != string.Empty)
            {
                if (Version == _searchVersion)
                {
                    Results.Clear();
                    foreach (var r in result.Take(200))
                    {
                        Results.Add(r);
                    }
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
        private void SearchBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Down:

                    if (FoundFiles.SelectedIndex < FoundFiles.Items.Count)
                    {
                        FoundFiles.SelectedIndex++;
                        FoundFiles.ScrollIntoView(userSelectedItem);
                    }
                    break;
                case System.Windows.Input.Key.Up:
                    if (FoundFiles.SelectedIndex > 0)
                    {
                        FoundFiles.SelectedIndex--;
                        FoundFiles.ScrollIntoView(userSelectedItem);
                    }
                    break;
                case System.Windows.Input.Key.Enter:
                    if (userSelectedItem != null)
                    {
                        if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Control))
                            _ioService.OpenFileFolder(userSelectedItem);
                        else
                            _ioService.OpenFile(userSelectedItem);
                    }
                    break;
                case System.Windows.Input.Key.Escape:
                    if (System.Windows.Input.Keyboard.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Shift))
                    {
                        this.Close();
                    }
                    else
                    {
                        FoundFiles.SelectedIndex = 0;
                        SearchBox.Text = "";
                        Results.Clear();
                    }
                    break;
            }
        }
    }
}