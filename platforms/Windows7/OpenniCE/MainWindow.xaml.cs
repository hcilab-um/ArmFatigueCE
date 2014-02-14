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
using System.Windows.Navigation;
using System.Windows.Shapes;
using OpenNIWrapper;
using NiTEWrapper;
using System.Threading;
using System.ComponentModel;
using WrapperCE;
using WrapperCE.InterOp;

namespace OpenNiCE
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region Private Value

		private const double TORQUE_MODIFIER = 2d;
		private SkeletonDrawer skeletonDrawer;
		private OpenKinect kinectSensor;
		private EngineCE engine;
		private ulong lastUpdate = 0;
		private double totalTimeInSeconds;
		private ArmFatigueUpdate armFatigueUpdate;
		private double leftCE;
		private double rightCE;
		private bool isEngineStart;
		private UserGender userGender;
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

		public double LeftCE
		{
			get { return leftCE; }
			set
			{
				leftCE = value;
				OnPropertyChanged("LeftCE");
			}
		}

		public double RightCE
		{
			get { return rightCE; }
			set
			{
				rightCE = value;
				OnPropertyChanged("RightCE");
			}
		}

		public bool IsEngineStart
		{
			get { return isEngineStart; }
			set
			{
				isEngineStart = value;
				OnPropertyChanged("IsEngineStart");
			}
		}

		#endregion

		public MainWindow()
		{
			userGender = UserGender.Male;
			engine = new EngineCE();
			engine.SetGender(userGender);
			TotalTimeInSeconds = 0;
			LeftCE = 0;
			RightCE = 0;
			IsEngineStart = false;
			kinectSensor = new OpenKinect();
			skeletonDrawer = new SkeletonDrawer(kinectSensor.SkeletonSensor);
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			kinectSensor.SkeletonSensor.OnNewData += new UserTracker.UserTrackerListenerDelegate(SkeletonSensor_OnNewData);
		}

		private void SkeletonSensor_OnNewData(UserTracker userTracker)
		{

			if (!userTracker.IsValid)
				return;

			ulong currentTimeTicks = 0;
			Skeleton validSkeleton = null;
			using (UserTrackerFrameRef frame = userTracker.ReadFrame())
			{
				if (frame == null || !frame.IsValid)
					return;

				foreach (UserData user in frame.Users)
				{
					if (user.IsNew && user.IsVisible)
					{
						userTracker.StartSkeletonTracking(user.UserId);
					}
					if (user.IsVisible && user.Skeleton.State == Skeleton.SkeletonState.Tracked)
					{
						validSkeleton = user.Skeleton;
						break;
					}
				}
				currentTimeTicks = frame.Timestamp;
			}

			if (validSkeleton != null && IsEngineStart)
			{
				double deltaTimeTicks = (currentTimeTicks - lastUpdate);
				if (lastUpdate == 0)
					deltaTimeTicks = 0;
				lastUpdate = currentTimeTicks;
				RunFatigueEngine(validSkeleton, deltaTimeTicks / 1000000);
			}

			Dispatcher.Invoke((Action)delegate
			{
				iKinectCapture.Source = kinectSensor.RawImageSource;
				if (validSkeleton != null)
				{
					iSkeleton.Source = DrawSkeleton(validSkeleton, Brushes.Transparent);
				}
				else
					iSkeleton.Source = null;
			});
		}

		private void RunFatigueEngine(Skeleton skeleton, double deltaTimeInSeconds)
		{
			TotalTimeInSeconds += deltaTimeInSeconds;
			SkeletonData measuredArms = new SkeletonData();
			measuredArms.RightShoulderCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.RightShoulder).Position);
			measuredArms.RightElbowCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.RightElbow).Position);
			measuredArms.RightHandCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.RightHand).Position);
			
			measuredArms.RightWristCms = engine.EstimateWristPosition(measuredArms.RightHandCms, measuredArms.RightElbowCms);

			measuredArms.LeftShoulderCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.LeftShoulder).Position);
			measuredArms.LeftElbowCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.LeftElbow).Position);
			measuredArms.LeftHandCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.LeftHand).Position);
			
			measuredArms.LeftWristCms = engine.EstimateWristPosition(measuredArms.LeftHandCms, measuredArms.LeftElbowCms);
			
			armFatigueUpdate = engine.ProcessNewSkeletonData(measuredArms, deltaTimeInSeconds);
			RightCE = armFatigueUpdate.RightArm.ConsumedEndurance;
			LeftCE = armFatigueUpdate.LeftArm.ConsumedEndurance;
		}

		private Point3D Convert(System.Windows.Media.Media3D.Point3D trackedPoint)
		{
			Point3D point3D;
			point3D.X = trackedPoint.X;
			point3D.Y = trackedPoint.Y;
			point3D.Z = trackedPoint.Z;
			return point3D;
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			kinectSensor.Dispose();
		}

		private DrawingImage DrawSkeleton(Skeleton skeleton, SolidColorBrush brush)
		{
			DrawingGroup dGroup = new DrawingGroup();
			using (DrawingContext dc = dGroup.Open())
			{
				dc.DrawRectangle(brush, new Pen(Brushes.Black, 0.5), new Rect(0, 0, kinectSensor.FrameWidth, kinectSensor.FrameHeight));
				skeletonDrawer.DrawSkeleton(skeleton, dc);
				skeletonDrawer.DrawCirlce(skeleton, NiTEWrapper.SkeletonJoint.JointType.RightShoulder, dc, armFatigueUpdate.RightArm.ArmStrength / TORQUE_MODIFIER);
			}

			DrawingImage dImageSource = new DrawingImage(dGroup);
			dGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, kinectSensor.FrameWidth, kinectSensor.FrameHeight));
			return dImageSource;
		}

		private void BtStart_Click(object sender, RoutedEventArgs e)
		{
			IsEngineStart = true;
			engine.Reset();
			TotalTimeInSeconds = 0;
			IsEngineStart = true;
			lastUpdate = 0;
		}

		private void BtStop_Click(object sender, RoutedEventArgs e)
		{
			IsEngineStart = false;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}