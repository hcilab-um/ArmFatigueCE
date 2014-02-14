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
//using System.Drawing;

namespace OpenNiCE
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region Private Value

		private SkeletonDrawer skeletonDrawer;
		private OpenKinect kinectSensor;
		private EngineCE engine;
		private ulong lastUpdate = 0;
		private double totalTimeInSeconds;
		private ArmFatigueUpdate armFatigueUpdate;
		
		private double leftCE;
		private double rightCE;
		private bool isEngineStart;

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
			engine = new EngineCE();

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

			UserData trackedUser = null;
			ulong currentTimeTicks = 0;

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
				}

				currentTimeTicks = frame.Timestamp;
				trackedUser = frame.Users.FirstOrDefault(user=>user.Skeleton.State == Skeleton.SkeletonState.Tracked);
			}


			if (trackedUser != null)
			{
				double deltaTimeMilliseconds = (currentTimeTicks - lastUpdate);
				lastUpdate = currentTimeTicks;
				if(IsEngineStart)
					RunFatigueEngine(trackedUser.Skeleton, deltaTimeMilliseconds / 1000000);
			}

			Dispatcher.Invoke((Action)delegate
			{
				iKinectCapture.Source = kinectSensor.RawImageSource;
				if (trackedUser != null)
					iSkeleton.Source = DrawSkeleton(trackedUser.Skeleton, Brushes.Transparent);
				else
					iSkeleton.Source = null;
			});
		}

		private void RunFatigueEngine(Skeleton skeleton, double deltaTimeInSeconds)
		{
			TotalTimeInSeconds += deltaTimeInSeconds;
			SkeletonData measuredArms = new SkeletonData();
			measuredArms.RightShoulderCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.RightShoulder).Position);
			measuredArms.RightElbowCms =    Convert(skeleton.GetJoint(SkeletonJoint.JointType.RightElbow).Position);
			measuredArms.RightWristCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.RightHand).Position);
			measuredArms.RightHandCms =     Convert(skeleton.GetJoint(SkeletonJoint.JointType.RightHand).Position);

			measuredArms.LeftShoulderCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.LeftShoulder).Position);
			measuredArms.LeftElbowCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.LeftElbow).Position);
			measuredArms.LeftWristCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.LeftHand).Position);
			measuredArms.LeftHandCms = Convert(skeleton.GetJoint(SkeletonJoint.JointType.LeftHand).Position);

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
				//if (CurrentFatigueInfo.SelectedArm == Arm.RightArm)
				//  skeletonDrawer.DrawCirlce(skeleton, JointType.ShoulderRight, dc, CurrentFatigueInfo.RightData.ArmStrength / TORQUE_MODIFIER);
				//else
				//  skeletonDrawer.DrawCirlce(skeleton, JointType.ShoulderLeft, dc, CurrentFatigueInfo.LeftData.ArmStrength / TORQUE_MODIFIER);
			}

			DrawingImage dImageSource = new DrawingImage(dGroup);
			dGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, kinectSensor.FrameWidth, kinectSensor.FrameHeight));
			return dImageSource;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private void BtStart_Click(object sender, RoutedEventArgs e)
		{
			IsEngineStart = true;
			engine.Reset();
			TotalTimeInSeconds = 0;
			IsEngineStart = true;
		}

		private void BtStop_Click(object sender, RoutedEventArgs e)
		{
			IsEngineStart = false;
		}

	}
}