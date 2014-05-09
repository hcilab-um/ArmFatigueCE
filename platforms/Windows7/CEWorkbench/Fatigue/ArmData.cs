using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using WrapperCE.InterOp;

namespace CEWorkbench.Fatigue
{
	public class ArmData : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private Arm arm;
		private double angle;
		private double shoulderTorque;
		private double armStrength;
		private double avgArmStrength;
		private double avgShoulderTorque;
		private double avgEndurance;
		private double consumedEndurance;
		
		#region Property
		public Arm Arm
		{
			get { return arm; }
			set
			{
				arm = value;
				OnPropertyChanged("Arm");
			}
		}

		public double Angle
		{
			get { return angle; }
			set
			{
				angle = value;
				OnPropertyChanged("Angle");
			}
		}

		public double ShoulderTorque
		{
			get { return shoulderTorque; }
			set
			{
				shoulderTorque = value;
				OnPropertyChanged("ShoulderTorque");
			}
		}

		public double ArmStrength
		{
			get { return armStrength; }
			set
			{
				armStrength = value;
				OnPropertyChanged("ArmStrength");
			}
		}

		public double AvgArmStrength
		{
			get { return avgArmStrength; }
			set
			{
				avgArmStrength = value;
				OnPropertyChanged("AvgArmStrength");
			}
		}

		public double AvgShoulderTorque
		{
			get { return avgShoulderTorque; }
			set
			{
				avgShoulderTorque = value;
				OnPropertyChanged("AvgShoulderTorque");
			}
		}

		public double AvgEndurance
		{
			get { return avgEndurance; }
			set
			{
				avgEndurance = value;
				OnPropertyChanged("AvgEndurance");
			}
		}

		public double ConsumedEndurance
		{
			get { return consumedEndurance; }
			set
			{
				consumedEndurance = value;
				OnPropertyChanged("ConsumedEndurance");
			}
		}
		#endregion

		public ArmData(Arm arm)
		{
			Arm = arm;
			Reset();
		}

		public ArmData(ArmData data)
		{
			Arm = data.Arm;
			Angle = data.Angle;
			ShoulderTorque = data.ShoulderTorque;
			ArmStrength = data.ArmStrength;
			AvgArmStrength = data.AvgArmStrength;
			AvgShoulderTorque = data.AvgShoulderTorque;
			AvgEndurance = data.AvgEndurance;
			ConsumedEndurance = data.ConsumedEndurance;
		}

		public void Reset()
		{
			Angle = 0;
			ShoulderTorque = 0;
			ArmStrength = 0;
			AvgArmStrength = 0;
			AvgShoulderTorque = 0;
			AvgEndurance = 0;
			ConsumedEndurance = 0;
		}

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
