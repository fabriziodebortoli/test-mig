using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Data;
using System.IO;
using System.Windows.Media.Imaging;

namespace EADesigner.ActivitiesDesigner
{
    public static class Utilities
    {
        public static ImageSource ConvertToBitmapIntoImageSource(Bitmap bitmap)
        {
            var converter = new WPFBitmapConverter();
            return (ImageSource)converter.Convert(bitmap, null, null, null);
        }
    }

    internal class WPFBitmapConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)value).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();

            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
