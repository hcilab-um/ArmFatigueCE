using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.IO;
using WrapperCE.InterOp;
using System.Windows.Forms;
using System.Diagnostics;

namespace DemoCE
{
	/// <summary>
	/// Interaction logic for SettingWindow.xaml
	/// </summary>
	public partial class SettingWindow : Window, INotifyPropertyChanged
	{
		#region Private Value
		private string recordPath;
		private Arm arm;
		private UserGender gender;
		#endregion

		public string RecordPath
		{
			get { return recordPath; }
			set
			{
				recordPath = value;
				OnPropertyChanged("RecordPath");
			}
		}

		public Arm Arm
		{
			get { return arm; }
			set
			{
				arm = value;
				OnPropertyChanged("Arm");
			}
		}

		public UserGender Gender
		{
			get { return gender; }
			set
			{
				gender = value;
				OnPropertyChanged("Gender");
			}
		}

		public SettingWindow()
		{
			RecordPath = Directory.GetCurrentDirectory();
			Gender = UserGender.Male;
			Arm = Arm.RightArm;
			InitializeComponent();
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private void btChangeDirectory_Click(object sender, RoutedEventArgs e)
		{
			var dialog = new FolderBrowserDialog();
			DialogResult result = dialog.ShowDialog();
			RecordPath = dialog.SelectedPath;
		}

		private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}
