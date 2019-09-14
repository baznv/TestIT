using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TestIT.Converter
{
    class StringToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //byte[] imgBytes = System.Convert.FromBase64String(value.ToString());

            //BitmapImage bitmapImage = new BitmapImage();
            //MemoryStream ms = new MemoryStream(imgBytes);
            //bitmapImage.BeginInit();
            //bitmapImage.StreamSource = ms;
            //bitmapImage.EndInit();
            if (value != null)
            {
                byte[] newBytes = System.Convert.FromBase64String(value.ToString());
                BitmapSource bitmap = (BitmapSource)new ImageSourceConverter().ConvertFrom(newBytes);
                return bitmap;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MemoryStream Memostr = new MemoryStream();
            byte[] arrayimg = Memostr.ToArray();
            return System.Convert.ToBase64String(arrayimg);
        }
    }
}
