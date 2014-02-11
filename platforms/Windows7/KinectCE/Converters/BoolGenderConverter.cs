using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using WrapperCE.InterOp;

namespace KinectCE.Converters
{
	public class BooleanGenderConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			UserGender target = (UserGender)value;
			UserGender actual = (UserGender)Enum.Parse(typeof(UserGender), parameter as String);

			if (actual == target)
				return true;
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isChecked = (bool)value;
			if (!isChecked)
				return null;

			return (UserGender)Enum.Parse(typeof(WrapperCE.InterOp.UserGender), parameter as String);
		}
	}
}
