using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace DemoCE.Converters
{
	public class ButtonEnableConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (values == DependencyProperty.UnsetValue || values[0] == DependencyProperty.UnsetValue
				|| values[1] == DependencyProperty.UnsetValue)
				return true;
			bool isRecording = (bool)values[0];
			bool isPlaying = (bool)values[1];
			string param = (string)parameter;
			bool enable = false;
			
			if (isPlaying || isRecording)
				enable = false;
			else
				enable = true;

			if (param.Equals("stop"))
				return !enable;
			return enable;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
