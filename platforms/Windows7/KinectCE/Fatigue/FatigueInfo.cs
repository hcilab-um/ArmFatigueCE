using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using WrapperCE.InterOp;

namespace KinectCE.Fatigue
{
	public class FatigueInfo : INotifyPropertyChanged
	{
		#region Private Value

		private double totalTimeInSeconds = 0;
		private DateTime dateTime;
		private string fatigueName;
		private string fatigueFile;
		private UserGender gender;
		private Arm selectedArm;

		#endregion

		#region Property

		public ArmData LeftData { get; set; }
		public ArmData RightData { get; set; }

		public double TotalTimeInSeconds
		{
			get { return totalTimeInSeconds; }
			set
			{
				totalTimeInSeconds = value;
				OnPropertyChanged("TotalTimeInSeconds");
			}
		}

		public DateTime DateTime
		{
			get { return dateTime; }
			set
			{
				dateTime = value;
				OnPropertyChanged("DateTime");
			}
		}

		public string FatigueName
		{
			get { return fatigueName; }
			set
			{
				fatigueName = value;
				OnPropertyChanged("FatigueFileName");
			}
		}

		public string FatigueFile
		{
			get { return fatigueFile; }
			set
			{
				fatigueFile = value;
				OnPropertyChanged("FatigueFile");
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

		public Arm SelectedArm
		{
			get { return selectedArm; }
			set
			{
				selectedArm = value;
				OnPropertyChanged("SelectedArm");
			}
		}
		#endregion

		public FatigueInfo()
		{
			DateTime = DateTime.Now;
			FatigueName = string.Empty;
			Gender = UserGender.Male;
			SelectedArm = Arm.RightArm;
			LeftData = new ArmData(Arm.LeftArm);
			RightData = new ArmData(Arm.RightArm);
			Reset();
		}

		public void Reset()
		{
			LeftData.Reset();
			RightData.Reset();
			TotalTimeInSeconds = 0;
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

	}
}
