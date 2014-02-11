using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace KinectCE.Converters
{
	public class HeightMarginConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == DependencyProperty.UnsetValue)
				return 0;

			double containerHeight = (double)value;
			int margin = Int32.Parse((String)parameter);

			return containerHeight - margin;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
