using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WrapperCE.InterOp;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DemoCE.Controls
{
  /// <summary>
  /// Interaction logic for TimelineControl.xaml
  /// </summary>
  public partial class TimelineControl : UserControl, INotifyPropertyChanged
  {
		public static readonly RoutedEvent DeleteFatigueInfoEvent = EventManager.RegisterRoutedEvent("DeleteFatigueInfo", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimelineControl));
		public static readonly RoutedEvent ReplayFatigueEvent = EventManager.RegisterRoutedEvent("ReplayFatigue", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimelineControl));


		public static readonly DependencyProperty TotalTimeInSecondsProperty = DependencyProperty.Register("TotalTimeInSeconds", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty LeftArmCEProperty = DependencyProperty.Register("LeftArmCE", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty RightArmCEProperty = DependencyProperty.Register("RightArmCE", typeof(double), typeof(TimelineControl));

		public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty LenghtInSecondsProperty = DependencyProperty.Register("LenghtInSeconds", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty IsEngineRunningProperty = DependencyProperty.Register("IsEngineRunning", typeof(bool), typeof(TimelineControl));

		private double timePlotValue;
		private double totalConsumeEndurance;

		public double TimePlotValue
		{
			get { return timePlotValue; }
			set
			{
				timePlotValue = value;
				OnPropertyChanged("TimePlotValue");
			}
		}

		public double TotalConsumeEndurance
		{
			get { return totalConsumeEndurance; }
			set
			{
				totalConsumeEndurance = value;
				OnPropertyChanged("TotalConsumeEndurance");
			}
		}
		
		public event RoutedEventHandler DeleteFatigueInfo
		{
			add { AddHandler(DeleteFatigueInfoEvent, value); }
			remove { RemoveHandler(DeleteFatigueInfoEvent, value); }
		}

		public bool IsEngineRunning
		{
			get { return (bool)GetValue(IsEngineRunningProperty); }
			set { SetValue(IsEngineRunningProperty, value); }
		}

		public double TotalTimeInSeconds
		{
			get { return (double)GetValue(TotalTimeInSecondsProperty); }
			set { SetValue(TotalTimeInSecondsProperty, value); }
		}

		public double LeftArmCE
		{
			get { return (double)GetValue(LeftArmCEProperty); }
			set { SetValue(LeftArmCEProperty, value); }
		}

		public double RightArmCE
		{
			get { return (double)GetValue(RightArmCEProperty); }
			set { SetValue(RightArmCEProperty, value); }
		}

		public double MaxValue
		{
			get { return (double)GetValue(MaxValueProperty); }
			set { SetValue(MaxValueProperty, value); }
		}

		public double LenghtInSeconds
		{
			get { return (double)GetValue(LenghtInSecondsProperty); }
			set { SetValue(LenghtInSecondsProperty, value); }
		}

		public event RoutedEventHandler ReplayFatigue
		{
			add { AddHandler(ReplayFatigueEvent, value); }
			remove { RemoveHandler(ReplayFatigueEvent, value); }
		}

		public TimelineControl()
		{
			InitializeComponent();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == TimelineControl.TotalTimeInSecondsProperty)
			{
			}
			else if (e.Property == TimelineControl.LeftArmCEProperty)
			{
				InsertNewDataPoint(LeftArmCE, Arm.LeftArm);
			}
			else if (e.Property == TimelineControl.RightArmCEProperty)
			{
				InsertNewDataPoint(RightArmCE, Arm.RightArm);
			}
		}

		private void InsertNewDataPoint(double consumeEndurance, WrapperCE.InterOp.Arm arm)
		{
			if (LenghtInSeconds == 0 || MaxValue == 0)
				return;
			TimePlotValue = TotalTimeInSeconds * (cGraphContent.Width) / LenghtInSeconds;
			Point newPoint = new Point(TimePlotValue, consumeEndurance * (cGraphContent.Height) / MaxValue);
			if (arm == WrapperCE.InterOp.Arm.RightArm)
				plotGraphRight.Points.Add(newPoint);
			else
				plotGraphLeft.Points.Add(newPoint);
		}

		private void BtDeleteClick(object sender, RoutedEventArgs e)
		{
			if (IsEngineRunning)
				return;
			RaiseEvent(new RoutedEventArgs(TimelineControl.DeleteFatigueInfoEvent, this));
		}

		private void BtReplayClick(object sender, RoutedEventArgs e)
		{
			if (IsEngineRunning)
				return;
			plotGraphLeft.Points.Clear();
			plotGraphRight.Points.Clear();
			RaiseEvent(new RoutedEventArgs(TimelineControl.ReplayFatigueEvent, this));
		}

		private void Button_MouseEnter(object sender, MouseEventArgs e)
		{
			Button button = (Button)sender;
			button.Opacity = 1;
		}

		private void Button_MouseLeave(object sender, MouseEventArgs e)
		{
			Button button = (Button)sender;
			button.Opacity = 0.05;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}

}
