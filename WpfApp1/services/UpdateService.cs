using Search.Domain;
using System.Windows.Media.Imaging;
using WpfApp1.Domain.Dto;

namespace WpfApp1.servives
{
    public class UpdateService
    {
        public PreviewDto Update(IndexedFile? file)
        {
            PreviewDto result = new PreviewDto();
            List<string> acceptableExtenstions = new List<string>
            { ".png", ".jpeg", ".bmp", ".gif", ".jpg" };

            if (acceptableExtenstions.Contains(file.Extension))
            {
                result.Image = ImagePreview(file);
            }
            result.ToolTip = file.ToString();

            return result;
        }
        private BitmapImage ImagePreview(IndexedFile file)
        {

            BitmapImage bitmapImage = new BitmapImage();
            try
            {
                bitmapImage.BeginInit();

                var path = file.FullPath.ToString();

                bitmapImage.UriSource = new Uri(path);
                bitmapImage.DecodePixelWidth = 200;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            catch (NotImplementedException)
            {

            }
            return bitmapImage;
        }
    }
}
