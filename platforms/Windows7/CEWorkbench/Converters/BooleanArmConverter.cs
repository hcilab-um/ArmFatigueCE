using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using WrapperCE.InterOp;
using System.Windows;

namespace CEWorkbench.Converters
{
	public class BooleanArmConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Arm target = (Arm)value;
			Arm actual = (Arm)Enum.Parse(typeof(Arm), parameter as String);
			if (actual == target)
				return true;
			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool isChecked = (bool)value;
			if (!isChecked)
				return null;

			return (Arm)Enum.Parse(typeof(Arm), parameter as String);
		}
	}
}
