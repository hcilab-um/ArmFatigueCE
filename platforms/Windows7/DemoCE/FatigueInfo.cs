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
		private string fatigueFileName;
		private WrapperCE.InterOp.UserGender gender;

		private double leftArmAngle = 0;
		private double rightArmAngle = 0;

		private double leftArmTorque = 0;
		private double rightArmTorque = 0;

		private double leftShoulderTorquePercent = 0;
		private double rightShoulderTorquePercent = 0;

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

		public string FatigueFileName
		{
			get { return fatigueFileName; }
			set
			{
				fatigueFileName = value;
				OnPropertyChanged("FatigueFileName");
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

		public double LeftShoulderTorquePercent
		{
			get { return leftShoulderTorquePercent; }
			set
			{
				leftShoulderTorquePercent = value;
				OnPropertyChanged("LeftShoulderTorquePercent");
			}
		}

		public double RightShoulderTorquePercent
		{
			get { return rightShoulderTorquePercent; }
			set
			{
				rightShoulderTorquePercent = value;
				OnPropertyChanged("RightShoulderTorquePercent");
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
			FatigueFileName = string.Empty;
			Gender = UserGender.Male;
			Reset();
		}

		public void Reset()
		{
			TotalTimeInSeconds = 0;
			LeftArmAngle = 0;
			RightArmAngle = 0;

			LeftArmTorque = 0;
			RightArmTorque = 0;

			LeftArmAvgTorque = 0;
			RightArmAvgTorque = 0;

			LeftShoulderTorquePercent = 0;
			RightShoulderTorquePercent = 0;

			LeftArmAvgEndurance = 0;
			RightArmAvgEndurance = 0;

			LeftArmConsumedEndurance = 0;
			RightArmConsumedEndurance = 0;
		}
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

	}
}
