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

namespace DemoCE.Controls
{
  /// <summary>
  /// Interaction logic for TimelineControl.xaml
  /// </summary>
  public partial class TimelineControl : UserControl
  {
		public static readonly DependencyProperty DeltaProperty = DependencyProperty.Register("Delta", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty LeftArmCEProperty = DependencyProperty.Register("LeftArmCE", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty RightArmCEProperty = DependencyProperty.Register("RightArmCE", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty LenghtInSecondsProperty = DependencyProperty.Register("LenghtInSeconds", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty GraphValueReferenceProperty = DependencyProperty.Register("GraphValueReference", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty CurrentFrameProperty = DependencyProperty.Register("CurrentFrame", typeof(int), typeof(TimelineControl));
		public static readonly DependencyProperty ValueOneVisibleProperty = DependencyProperty.Register("ValueOneVisible", typeof(Visibility), typeof(TimelineControl));
		public static readonly DependencyProperty IsEngineRunningProperty = DependencyProperty.Register("IsEngineRunning", typeof(bool), typeof(TimelineControl));
		
		private double elapsedTime = 0;

		public bool IsEngineRunning
		{
			get { return (bool)GetValue(IsEngineRunningProperty); }
			set { SetValue(IsEngineRunningProperty, value); }
		}

		public double Delta
		{
			get { return (double)GetValue(DeltaProperty); }
			set { SetValue(DeltaProperty, value); }
		}

		public double LeftArmCE
		{
			get { return (double)GetValue(LeftArmCEProperty); }
			set { SetValue(LeftArmCEProperty, value); }
		}

		public double RightArmCE
		{
			get { return (double)GetValue(RightArmCEProperty); }
			set { SetValue(LeftArmCEProperty, value); }
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

		public double GraphValueReference
		{
			get { return (double)GetValue(GraphValueReferenceProperty); }
			set { SetValue(GraphValueReferenceProperty, value); }
		}

		public int CurrentFrame
		{
			get { return (int)GetValue(CurrentFrameProperty); }
			set { SetValue(CurrentFrameProperty, value); }
		}

		public TimelineControl()
		{
			InitializeComponent();
		}

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);
			if (e.Property == TimelineControl.DeltaProperty)
			{
				elapsedTime += Delta;
			}
			else if (e.Property == TimelineControl.LeftArmCEProperty)
			{
				InsertNewDataPoint(LeftArmCE, Arm.LeftArm, Brushes.Green);
			}
			else if (e.Property == TimelineControl.RightArmCEProperty)
			{
				InsertNewDataPoint(RightArmCE,  Arm.RightArm, Brushes.Yellow);
			}
			else if (e.Property == TimelineControl.MaxValueProperty || e.Property == TimelineControl.ActualHeightProperty)
			{
				double axisLenght = lAxisY.ActualHeight - lAxisX.Margin.Bottom;
			}
			else if (e.Property == TimelineControl.LenghtInSecondsProperty || e.Property == TimelineControl.ActualWidthProperty)
			{
				double axisLenght = lAxisX.ActualWidth - lAxisY.Margin.Left;
			}
			else if (e.Property == TimelineControl.IsEngineRunningProperty)
			{
				bool engineRunning = (bool)e.NewValue;
				if (!engineRunning)
					return;
				cGraphContent.Children.Clear();
				elapsedTime = 0;
			}
		}

		private void InsertNewDataPoint(double value, WrapperCE.InterOp.Arm arm, Brush colorBrush)
		{
			Ellipse newPoint = new Ellipse();
			newPoint.Width = 2;
			newPoint.Height = 4;
			newPoint.Fill = colorBrush;
			Canvas.SetLeft(newPoint, elapsedTime * (ActualWidth - 33) / LenghtInSeconds);
			Canvas.SetBottom(newPoint, value * (ActualHeight - 30) / MaxValue);
			cGraphContent.Children.Add(newPoint);
			newPoint.ToolTip = new ToolTip() { Content = String.Format("Arm: {0}\nValue: {1}", arm, value) };
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{

		}
  }

}
