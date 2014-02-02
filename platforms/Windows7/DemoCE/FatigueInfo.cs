using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using WrapperCE.InterOp;

namespace DemoCE
{
	public class FatigueInfo : INotifyPropertyChanged
	{
		#region Private Value

		private double totalTimeInSeconds = 0;
		private string fatigueName;
		private string fatigueFile;
		private UserGender gender;
		private Arm arm;

		private double leftArmAngle = 0;
		private double rightArmAngle = 0;

		private double leftArmTorque = 0;
		private double rightArmTorque = 0;

		private double leftArmStrength = 0;
		private double rightArmStrength = 0;

		private double rightArmAvgStrength = 0;
		private double leftArmAvgStrength = 0;

		private double leftArmAvgTorque = 0;
		private double rightArmAvgTorque = 0;

		private double leftArmAvgEndurance = 0;
		private double rightArmAvgEndurance = 0;

		private double leftArmConsumedEndurance = 0;
		private double rightArmConsumedEndurance = 0;

		#endregion

		#region Property

		public double TotalTimeInSeconds
		{
			get { return totalTimeInSeconds; }
			set
			{
				totalTimeInSeconds = value;
				OnPropertyChanged("TotalTimeInSeconds");
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

		public Arm Arm
		{
			get { return arm; }
			set
			{
				arm = value;
				OnPropertyChanged("Arm");
			}
		}

		public double LeftArmAngle
		{
			get { return leftArmAngle; }
			set
			{
				leftArmAngle = value;
				OnPropertyChanged("LeftArmAngle");
			}
		}

		public double RightArmAngle
		{
			get { return rightArmAngle; }
			set
			{
				rightArmAngle = value;
				OnPropertyChanged("RightArmAngle");
			}
		}

		public double LeftArmTorque
		{
			get { return leftArmTorque; }
			set
			{
				leftArmTorque = value;
				OnPropertyChanged("LeftArmTorque");
			}
		}

		public double RightArmTorque
		{
			get { return rightArmTorque; }
			set
			{
				rightArmTorque = value;
				OnPropertyChanged("RightArmTorque");
			}
		}

		public double LeftArmStrength
		{
			get { return leftArmStrength; }
			set
			{
				leftArmStrength = value;
				OnPropertyChanged("LeftArmStrength");
			}
		}

		public double RightArmStrength
		{
			get { return rightArmStrength; }
			set
			{
				rightArmStrength = value;
				OnPropertyChanged("RightArmStrength");
			}
		}

		public double LeftArmAvgStrength
		{
			get { return leftArmAvgStrength; }
			set
			{
				leftArmAvgStrength = value;
				OnPropertyChanged("LeftArmAvgStrength");
			}
		}

		public double RightArmAvgStrength
		{
			get { return rightArmAvgStrength; }
			set
			{
				rightArmAvgStrength = value;
				OnPropertyChanged("RightArmAvgStrength");
			}
		}


		public double LeftArmAvgTorque
		{
			get { return leftArmAvgTorque; }
			set
			{
				leftArmAvgTorque = value;
				OnPropertyChanged("LeftArmAvgTorque");
			}
		}

		public double RightArmAvgTorque
		{
			get { return rightArmAvgTorque; }
			set
			{
				rightArmAvgTorque = value;
				OnPropertyChanged("RightArmAvgTorque");
			}
		}

		public double LeftArmAvgEndurance
		{
			get { return leftArmAvgEndurance; }
			set
			{
				leftArmAvgEndurance = value;
				OnPropertyChanged("LeftArmAvgEndurance");
			}
		}

		public double RightArmAvgEndurance
		{
			get { return rightArmAvgEndurance; }
			set
			{
				rightArmAvgEndurance = value;
				OnPropertyChanged("RightArmAvgEndurance");
			}
		}

		public double LeftArmConsumedEndurance
		{
			get { return leftArmConsumedEndurance; }
			set
			{
				leftArmConsumedEndurance = value;
				OnPropertyChanged("LeftArmConsumedEndurance");
			}
		}

		public double RightArmConsumedEndurance
		{
			get { return rightArmConsumedEndurance; }
			set
			{
				rightArmConsumedEndurance = value;
				OnPropertyChanged("RightArmConsumedEndurance");
			}
		}
		#endregion

		public FatigueInfo()
		{
			FatigueName = string.Empty;
			Gender = UserGender.Male;
			Arm = Arm.RightArm;
			Reset();
		}

		public void Reset()
		{
			LeftArmAngle = 0;
			RightArmAngle = 0;

			LeftArmTorque = 0;
			RightArmTorque = 0;

			LeftArmAvgTorque = 0;
			RightArmAvgTorque = 0;

			LeftArmStrength = 0;
			RightArmStrength = 0;

			LeftArmAvgEndurance = 0;
			RightArmAvgEndurance = 0;

			LeftArmConsumedEndurance = 0;
			RightArmConsumedEndurance = 0;
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
