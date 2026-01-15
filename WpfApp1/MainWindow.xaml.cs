using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfApp1.Domain;
using WpfApp1.servives;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Uri Path;
        private readonly SearchService _service;
        private ObservableCollection<FileTypeChecker> fileTypeOptions;
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
            _service = new SearchService();
            PopulateList();

        }

        private async void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            var searchLocation = SearchLocation.Text.ToString();
            if (searchLocation == string.Empty)
            {
                MessageBox.Show("Empty Location, please fill location");
                return;
            }

            Files.Clear();
            SearchProgressBar.Visibility = Visibility.Visible;
            var selectedTypes = FileTypeOptions.Where(s => s.IsChecked == true).ToList();

            Progress<FileInfo> progress = new Progress<FileInfo>(file => Files.Add(file));

            await Task.Run(() => _service.SearchForImages(selectedTypes, progress, searchLocation));

            SearchProgressBar.Visibility = Visibility.Hidden;
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

        private void FoundFiles_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            List<string> acceptableExtenstions = new List<string> { ".png", ".jpeg", ".bmp", ".gif", ".jpg" };
            var file = FoundFiles.SelectedItem as FileInfo;
            if (file == null)
            {
                return;
            }
            else if (acceptableExtenstions.Contains(file.Extension))
            {
                try
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();

                    var path = file.FullName.ToString();

                    bitmapImage.UriSource = new Uri(path);
                    bitmapImage.DecodePixelWidth = 200;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    PreviewImage.Source = bitmapImage;
                }
                catch (NotSupportedException)
                {
                    PreviewImage.Source = null;
                    MessageBox.Show(file.FullName);
                }
            }

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
    }
}