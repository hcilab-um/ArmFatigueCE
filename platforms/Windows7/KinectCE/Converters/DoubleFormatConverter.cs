using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace KinectCE.Converters
{
  public class DoubleFormatConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      if (value == DependencyProperty.UnsetValue)
        return "0.00";

      double valueToConvert = (double)value;
      if (valueToConvert == Double.MaxValue)
        return Double.PositiveInfinity;

      int decimalPoints = Int32.Parse((String)parameter);
			if (decimalPoints == 0)
				return ((int)valueToConvert).ToString();

      String format = "F" + decimalPoints;
      String returnValue = valueToConvert.ToString(format);

      return returnValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
