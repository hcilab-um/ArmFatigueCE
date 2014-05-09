using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace CEWorkbench.Util
{
	public class ColorImageReadyArgs : EventArgs
	{
		public ImageSource Frame { get; set; }
	}
}
