using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
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
        private readonly UpdateService _update;
        private ICollectionView _filesView;
        private string _searchText = "";

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
            _service = new SearchService();
            _update = new UpdateService();
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
            var searchLocation = SearchLocation.Text.ToString();
            if (searchLocation == string.Empty)
            {
                searchLocation = @"C:\";
            }

            Files.Clear();
            SearchProgressBar.Visibility = Visibility.Visible;
            var selectedTypes = FileTypeOptions.Where(s => s.IsChecked == true).ToList();

            Progress<FileInfo> progress = new Progress<FileInfo>(file => Files.Add(file));

            await Task.Run(() => _service.SearchForImages(selectedTypes, progress, searchLocation));

            SearchProgressBar.Visibility = Visibility.Hidden;
        }
        private void FoundFiles_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //List<string> acceptableExtenstions = new List<string> { ".png", ".jpeg", ".bmp", ".gif", ".jpg" };
            //var file = FoundFiles.SelectedItem as FileInfo;
            //if (file == null)
            //{
            //    return;
            //}
            //else if (acceptableExtenstions.Contains(file.Extension))
            //{
            //    try
            //    {
            //        BitmapImage bitmapImage = new BitmapImage();
            //        bitmapImage.BeginInit();

            //        var path = file.FullName.ToString();

            //        bitmapImage.UriSource = new Uri(path);
            //        bitmapImage.DecodePixelWidth = 200;
            //        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            //        bitmapImage.EndInit();

            //        PreviewImage.Source = bitmapImage;
            //    }
            //    catch (NotSupportedException)
            //    {
            //        PreviewImage.Source = null;
            //        MessageBox.Show(file.FullName);
            //    }
            //}

        }

        private void btnPickLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            var dialogResul = dialog.ShowDialog();

            var selectedFolder = dialog.FolderName;

            if (selectedFolder == string.Empty)
            {
                SearchLocation.Text = "empty"; return;
            }

            SearchLocation.Text = selectedFolder;

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
            _searchText = SearchBox.Text;
            _filesView.Refresh();
        }
    }
}