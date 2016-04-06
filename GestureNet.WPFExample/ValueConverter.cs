using System;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using GestureNet.Recognisers;

namespace GestureNet.WPFExample
{
    public class ValueConverter : IMultiValueConverter
    {
        public string Threshhold { get; set; }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var result = values.FirstOrDefault() as Result;

            float threshold;
            var textBox = values.Skip(1).FirstOrDefault() as TextBox;

            if (textBox != null && float.TryParse(textBox.Text, out threshold))
                return result != null && result.Score < threshold;
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}