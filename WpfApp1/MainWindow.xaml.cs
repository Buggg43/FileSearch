using Search.services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfApp1.Domain;
using WpfApp1.servives;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public Uri Path;
        private readonly SearchService _service;
        private ObservableCollection<FileTypeChecker> fileTypeOptions;
        private FileInfo selectedFile;
        private FileParserService _fileParser;
        private readonly UpdateService _update;
        private ICollectionView _filesView;
        private string _searchText = "";
        private DispatcherTimer _dispatcherTimer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public BitmapImage PreviewSource { get; set; } = null;
        public string PreviewToolTip { get; set; } = "";
        public FileInfo SelectedFile
        {
            get
            {
                return selectedFile;
            }
            set
            {
                selectedFile = value;
                var result = _update.Update(value);
                if (result.Image != null)
                    PreviewSource = result.Image;
                else
                    PreviewSource = null;

                PreviewToolTip = result.ToolTip;

                OnPropertyChange(nameof(PreviewSource));
                OnPropertyChange(nameof(PreviewToolTip));
            }
        }
        public ObservableCollection<FileTypeChecker> FileTypeOptions
        {
            get
            {
                return fileTypeOptions;
            }
            set
            {
                fileTypeOptions = value;
            }
        }
        public ObservableCollection<FileInfo> Files { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Files = new ObservableCollection<FileInfo>();
            _filesView = CollectionViewSource.GetDefaultView(Files);
            _filesView.Filter = FilterFiles;
            _fileParser = new FileParserService();
            _service = new SearchService();
            _update = new UpdateService();
            _dispatcherTimer = new DispatcherTimer();

            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(300);
            _dispatcherTimer.Tick += OnTimedEvent;

            PopulateList();
        }
        private void PopulateList()
        {
            ObservableCollection<FileTypeChecker> typeCheckers = new ObservableCollection<FileTypeChecker>();
            var Types = Enum.GetValues<FileTypeEnum>();
            foreach (var type in Types)
            {
                typeCheckers.Add(new() { FileType = type, IsChecked = false });
            }
            FileTypeOptions = typeCheckers;
        }
        private bool FilterFiles(object obj)
        {
            if (obj is not FileInfo f) return false;
            if (string.IsNullOrWhiteSpace(_searchText)) return true;

            var q = _searchText.Trim();

            return f.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || f.FullName.Contains(q, StringComparison.OrdinalIgnoreCase);
        }
        private async void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            Files.Clear();
            SearchProgressBar.Visibility = Visibility.Visible;
            var selectedTypes = FileTypeOptions.Where(s => s.IsChecked == true).ToList();

            Progress<FileInfo> progress = new Progress<FileInfo>(file => Files.Add(file));

            await Task.Run(() => _service.SearchFiles());

            SearchProgressBar.Visibility = Visibility.Hidden;
        }
        private void OnTimedEvent(object sender, EventArgs e)
        {
            _dispatcherTimer.Stop();
            _filesView.Refresh();
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = (ListBoxItem)sender;
            var file = item.DataContext as FileInfo;
            if (file == null) return;

            Process.Start(new ProcessStartInfo
            {
                FileName = file.FullName,
                UseShellExecute = true
            });
        }
        private void OnPropertyChange([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        private void SearchBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.Key;
            if (key == Key.Up && 0 < FoundFiles.SelectedIndex && FoundFiles.Items.Count > 0)
            {
                e.Handled = true;
                FoundFiles.SelectedIndex--;
                FoundFiles.ScrollIntoView(FoundFiles.SelectedItem);
            }
            else if (key == Key.Down && FoundFiles.Items.Count > 0)
            {
                if (FoundFiles.SelectedIndex == -1)
                {
                    e.Handled = true;
                    FoundFiles.SelectedIndex = 0;
                    FoundFiles.ScrollIntoView(FoundFiles.SelectedItem);
                }
                else if (FoundFiles.Items.Count - 1 > FoundFiles.SelectedIndex)
                {
                    e.Handled = true;
                    FoundFiles.SelectedIndex++;
                    FoundFiles.ScrollIntoView(FoundFiles.SelectedItem);
                }
            }
            else if (key == Key.Enter)
            {
                if (FoundFiles.Items.Count > 0)
                {
                    e.Handled = true;
                    if (FoundFiles.SelectedIndex == -1)
                    {
                        FoundFiles.SelectedIndex = 0;
                        FoundFiles.ScrollIntoView(FoundFiles.SelectedItem);
                    }
                    if (FoundFiles.SelectedItem is FileInfo file)
                    {
                        if (Keyboard.Modifiers == ModifierKeys.Control)
                        {
                            OpenFileFolder(file);
                            this.Close();
                        }

                        else
                        {
                            OpenFile(file);
                            this.Close();
                        }
                    }

                }
            }
        }
        private void OpenFile(FileInfo file)
        {
            var process = new ProcessStartInfo()
            {
                FileName = file.FullName,
                UseShellExecute = true
            };
            Process.Start(process);
        }
        private void OpenFileFolder(FileInfo file)
        {
            var process = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{file.FullName}\"",
                UseShellExecute = true
            };
            Process.Start(process);
        }
    }
}