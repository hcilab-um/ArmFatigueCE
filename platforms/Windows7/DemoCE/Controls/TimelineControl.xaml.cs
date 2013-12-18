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

namespace DemoCE.Controls
{
  /// <summary>
  /// Interaction logic for TimelineControl.xaml
  /// </summary>
  public partial class TimelineControl : UserControl
  {

		public static readonly DependencyProperty DeltaProperty = DependencyProperty.Register("Delta", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty GraphValueOneProperty = DependencyProperty.Register("GraphValueOne", typeof(double), typeof(TimelineControl));

		public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register("MaxValue", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty LenghtInMinutesProperty = DependencyProperty.Register("LenghtInMinutes", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty DynamicLenghtProperty = DependencyProperty.Register("DynamicLenght", typeof(bool), typeof(TimelineControl));
		public static readonly DependencyProperty GraphValueReferenceProperty = DependencyProperty.Register("GraphValueReference", typeof(double), typeof(TimelineControl));
		public static readonly DependencyProperty CurrentFrameProperty = DependencyProperty.Register("CurrentFrame", typeof(int), typeof(TimelineControl));
		public static readonly DependencyProperty ValueOneVisibleProperty = DependencyProperty.Register("ValueOneVisible", typeof(Visibility), typeof(TimelineControl));

		public static readonly DependencyProperty IsEngineRunningProperty = DependencyProperty.Register("IsEngineRunning", typeof(bool), typeof(TimelineControl));

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

		public double GraphValueOne
		{
			get { return (double)GetValue(GraphValueOneProperty); }
			set { SetValue(GraphValueOneProperty, value); }
		}

		public Visibility ValueOneVisible
		{
			get { return (Visibility)GetValue(ValueOneVisibleProperty); }
			set { SetValue(ValueOneVisibleProperty, value); }
		}

		public double MaxValue
		{
			get { return (double)GetValue(MaxValueProperty); }
			set { SetValue(MaxValueProperty, value); }
		}

		public double LenghtInMinutes
		{
			get { return (double)GetValue(LenghtInMinutesProperty); }
			set { SetValue(LenghtInMinutesProperty, value); }
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

		public bool DynamicLenght
		{
			get { return (bool)GetValue(DynamicLenghtProperty); }
			set { SetValue(DynamicLenghtProperty, value); }
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
				if (DynamicLenght && elapsedTime > (LenghtInMinutes * 60))
					LenghtInMinutes += 0.02;
			}
			else if (e.Property == TimelineControl.GraphValueOneProperty)
			{
				InsertNewDataPoint(GraphValueOne, Brushes.Red);
			}
			else if (e.Property == TimelineControl.ValueOneVisibleProperty)
			{
				foreach (UIElement ui in cGraphContent.Children)
				{
					if (ui is Ellipse && ((Ellipse)ui).Fill == Brushes.Red)
						ui.Visibility = ValueOneVisible;
				}
			}
			else if (e.Property == TimelineControl.MaxValueProperty || e.Property == TimelineControl.ActualHeightProperty)
			{
				double axisLenght = lAxisY.ActualHeight - lAxisX.Margin.Bottom;
			}
			else if (e.Property == TimelineControl.LenghtInMinutesProperty || e.Property == TimelineControl.ActualWidthProperty)
			{
				double axisLenght = lAxisX.ActualWidth - lAxisY.Margin.Left;
			}
			else if (e.Property == TimelineControl.IsEngineRunningProperty)
			{
				bool engineRunning = (bool)e.NewValue;
				if (!engineRunning)
					return;

				var ellipses = new List<Ellipse>();
				foreach (UIElement element in cGraphContent.Children)
				{
					if (element is Ellipse)
						ellipses.Add(element as Ellipse);
				}
				foreach (Ellipse ellipse in ellipses)
					cGraphContent.Children.Remove(ellipse);

				elapsedTime = 0;
			}
		}

		private double elapsedTime = 0;
		private void InsertNewDataPoint(double value, Brush colorBrush)
		{
			Ellipse newPoint = new Ellipse();
			newPoint.Width = 2;
			newPoint.Height = 4;
			newPoint.Fill = colorBrush;
			Canvas.SetLeft(newPoint, elapsedTime);
			Canvas.SetTop(newPoint, value);
			cGraphContent.Children.Add(newPoint);
			newPoint.ToolTip = new ToolTip() { Content = String.Format("Frame: {0}\n Value: {1}", CurrentFrame, value) };
		}

  }

}
