using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Witcher3MapViewer
{

    public class RowColorConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            int value = (int)values[0];
            int level = (int)values[1];
            int result = level - value;
            if (result >= 0)
                return new SolidColorBrush(Color.FromRgb(255, 255, 255));
            if (result == -1)
                return new SolidColorBrush(Color.FromRgb(216, 180, 180));
            if (result == -2)
                return new SolidColorBrush(Color.FromRgb(216, 160, 160));
            if (result == -3)
                return new SolidColorBrush(Color.FromRgb(216, 140, 140));
            if (result == -4)
                return new SolidColorBrush(Color.FromRgb(216, 120, 120));
            return new SolidColorBrush(Color.FromRgb(216, 100, 100));
        }

       
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }        
    }

}
