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
using System.IO;
using DemoCE.Fatigue;

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

		public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty LenghtInSecondsProperty = DependencyProperty.Register("LenghtInSeconds", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty IsRecordingProperty = DependencyProperty.Register("IsRecording", typeof(bool), typeof(TimelineControl));
		public static readonly DependencyProperty IsPlayingProperty = DependencyProperty.Register("IsPlaying", typeof(bool), typeof(TimelineControl));
		private double timePlotValue;
		private double consumeEndurance;
		private List<FatigueInfo> fatigueInfoList;

		public double TimePlotValue
		{
			get { return timePlotValue; }
			set
			{
				timePlotValue = value;
				OnPropertyChanged("TimePlotValue");
			}
		}

		public double ConsumeEndurance
		{
			get { return consumeEndurance; }
			set
			{
				consumeEndurance = value;
				OnPropertyChanged("ConsumeEndurance");
			}
		}

		public event RoutedEventHandler DeleteFatigueInfo
		{
			add { AddHandler(DeleteFatigueInfoEvent, value); }
			remove { RemoveHandler(DeleteFatigueInfoEvent, value); }
		}

		public bool IsRecording
		{
			get { return (bool)GetValue(IsRecordingProperty); }
			set { SetValue(IsRecordingProperty, value); }
		}

		public bool IsPlaying
		{
			get { return (bool)GetValue(IsPlayingProperty); }
			set { SetValue(IsPlayingProperty, value); }
		}

		public double TotalTimeInSeconds
		{
			get { return (double)GetValue(TotalTimeInSecondsProperty); }
			set { SetValue(TotalTimeInSecondsProperty, value); }
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
			fatigueInfoList = new List<FatigueInfo>();
			InitializeComponent();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == TimelineControl.TotalTimeInSecondsProperty)
				InsertNewDataPoint((FatigueInfo)this.DataContext);
		}

		private void InsertNewDataPoint(FatigueInfo fatigueInfo)
		{
			if (LenghtInSeconds == 0 || MaxValue == 0 || TotalTimeInSeconds == 0)
				return;

			if (fatigueInfo.SelectedArm == Arm.LeftArm)
				ConsumeEndurance = fatigueInfo.LeftData.ConsumedEndurance;
			else
				ConsumeEndurance = fatigueInfo.RightData.ConsumedEndurance;

			//Shrink the graph if needed
			if (TotalTimeInSeconds > LenghtInSeconds)
			{
				for (int i = 0; i < plotGraphRight.Points.Count; i++)
				{
					plotGraphRight.Points[i] = new Point()
					{
						X = plotGraphRight.Points[i].X * LenghtInSeconds / TotalTimeInSeconds,
						Y = plotGraphRight.Points[i].Y
					};
				}
				LenghtInSeconds = TotalTimeInSeconds;
			}

			if (ConsumeEndurance > MaxValue)
			{
				for (int i = 0; i < plotGraphRight.Points.Count; i++)
				{
					plotGraphRight.Points[i] = new Point()
					{
						X = plotGraphRight.Points[i].X,
						Y = plotGraphRight.Points[i].Y * MaxValue / ConsumeEndurance
					};
				}
				MaxValue = ConsumeEndurance;
			}

			TimePlotValue = TotalTimeInSeconds * (cGraphContent.Width) / LenghtInSeconds;

			FatigueInfo newFatigueInfo = new FatigueInfo()
			{
				DateTime = DateTime.Now,
				FatigueFile = fatigueInfo.FatigueFile,
				SelectedArm = fatigueInfo.SelectedArm,
				TotalTimeInSeconds = fatigueInfo.TotalTimeInSeconds,
				LeftData = fatigueInfo.LeftData,
				RightData = fatigueInfo.RightData,
			};

			fatigueInfoList.Add(newFatigueInfo);

			Point newPoint = new Point(TimePlotValue, ConsumeEndurance * (cGraphContent.Height) / MaxValue);
			plotGraphRight.Points.Add(newPoint);
		}

		private void BtDeleteClick(object sender, RoutedEventArgs e)
		{
			if (IsRecording || IsPlaying)
			{
				MessageBox.Show("Can not delete when playing or recording fatigue data");
				return;
			}

			RaiseEvent(new RoutedEventArgs(TimelineControl.DeleteFatigueInfoEvent, this));
		}

		private void BtReplayClick(object sender, RoutedEventArgs e)
		{
			if (IsRecording)
				return;
			plotGraphRight.Points.Clear();
			fatigueInfoList.Clear();
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

		private void OnMouseShowPathInfo(object sender, MouseEventArgs e)
		{
			var polyLine = (Polyline)sender;
			Point mousePos = Mouse.GetPosition(cGraphContent);
			double timeInSecond = mousePos.X * LenghtInSeconds / (cGraphContent.Width);

			if (fatigueInfoList.Count == 0)
			{
				polyLine.ToolTip = "No Fatigue Information";
				return;
			}

			var selectedFatigue = fatigueInfoList.OrderBy(fatigue => Math.Abs(fatigue.TotalTimeInSeconds - timeInSecond)).First();
			string averageEndurance = "Infinity";

			if (selectedFatigue.SelectedArm == Arm.RightArm)
			{
				if (selectedFatigue.RightData.AvgEndurance < 1000)
					averageEndurance = selectedFatigue.RightData.AvgEndurance.ToString("F2");
				polyLine.ToolTip = string.Format("CE: {0} %\nTime: {1} sec\nAvg Strength: {2} %\nAvg Endurance: {3} sec",
																	selectedFatigue.RightData.ConsumedEndurance.ToString("F2"),
																	selectedFatigue.TotalTimeInSeconds.ToString("F2"),
																	selectedFatigue.RightData.ArmStrength.ToString("F2"),
																	averageEndurance);
			}
			else
			{
				if (selectedFatigue.LeftData.AvgEndurance < 1000)
					averageEndurance = selectedFatigue.LeftData.AvgEndurance.ToString("F2");
				polyLine.ToolTip = string.Format("CE: {0} %\nTime: {1} sec\nAvg Strength: {2} %\nAvg Endurance: {3} sec",
																	selectedFatigue.LeftData.ConsumedEndurance.ToString("F2"),
																	selectedFatigue.TotalTimeInSeconds.ToString("F2"),
																	selectedFatigue.LeftData.ArmStrength.ToString("F2"),
																	averageEndurance);
			}
		}

		public string GetEffortLog()
		{
			FatigueInfo fatigueInfo = fatigueInfoList.Last();
			Object[] logObjects = new Object[]
      {
				fatigueInfo.DateTime.ToString(),
				fatigueInfo.TotalTimeInSeconds,
				
				fatigueInfo.LeftData.AvgArmStrength,
				fatigueInfo.LeftData.AvgEndurance,
				fatigueInfo.LeftData.ConsumedEndurance,

				fatigueInfo.RightData.AvgArmStrength,
				fatigueInfo.RightData.AvgEndurance,
				fatigueInfo.RightData.ConsumedEndurance
      };

			int count = 0;
			StringBuilder formatSt = new StringBuilder();
			foreach (Object obj in logObjects)
				formatSt.Append("{" + (count++) + "},");
			return String.Format(formatSt.ToString(), logObjects);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}

}
