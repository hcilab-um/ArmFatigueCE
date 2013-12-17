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
using Microsoft.Kinect;
using System.ComponentModel;
using System.Windows.Media.Effects;

namespace DemoCE
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{

		private KinectSensor kinectSensor = null;
		private WriteableBitmap colorBitmap;
		private byte[] colorPixels;
		private SkeletonDrawer skeletonDrawer;
		private long lastUpdate = -1;
		private bool isKinectConnected = false;
		public bool IsKinectConnected
		{
			get { return isKinectConnected; }
			set
			{
				isKinectConnected = value;
				OnPropertyChanged("IsKinectConnected");
			}
		}

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (KinectSensor.KinectSensors.Count == 0)
			{
				IsKinectConnected = false;
				MessageBox.Show("No Kinect Sensor Detected");
			}
			else
			{
				IsKinectConnected = true;
				kinectSensor = KinectSensor.KinectSensors[0];
				if (kinectSensor == null)
					IsKinectConnected = false;
				else
				{
					kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
					colorPixels = new byte[kinectSensor.ColorStream.FramePixelDataLength];
					colorBitmap = new WriteableBitmap(kinectSensor.ColorStream.FrameWidth, kinectSensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
					iKinectCapture.Source = colorBitmap;

					kinectSensor.SkeletonStream.Enable();
					kinectSensor.DepthStream.Enable();
					kinectSensor.Start();
					kinectSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinectSensor_AllFramesReady);

					skeletonDrawer = new SkeletonDrawer(kinectSensor);
				}
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (null != kinectSensor)
			{
				kinectSensor.Stop();
			}
		}

		void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			long currentTimeMilliseconds = -1;
			Skeleton[] skeletons = null;
			Skeleton validSkeleton = null;
			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame != null)
				{
					skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
					skeletonFrame.CopySkeletonDataTo(skeletons);
					currentTimeMilliseconds = skeletonFrame.Timestamp;

					foreach (Skeleton skeleton in skeletons)
					{
						if (skeleton.TrackingState != SkeletonTrackingState.Tracked)
							continue;
						validSkeleton = skeleton;
						break;
					}
				}
			}

			if (validSkeleton != null)
			{
				double deltaTimeInSeconds = (currentTimeMilliseconds - lastUpdate) / 1000;
				if (lastUpdate == -1)
					deltaTimeInSeconds = 0;
				lastUpdate = currentTimeMilliseconds;

				WrapperCE.EngineCE engine = new WrapperCE.EngineCE();
				WrapperCE.InterOp.SkeletonData measuredArms = new WrapperCE.InterOp.SkeletonData();
				measuredArms.RightShoulderCms = Convert(validSkeleton.Joints[JointType.ShoulderRight].Position);
				measuredArms.RightElbowCms = Convert(validSkeleton.Joints[JointType.ElbowRight].Position);
				measuredArms.RightHandCms = Convert(validSkeleton.Joints[JointType.HandRight].Position);
				measuredArms.LeftShoulderCms = Convert(validSkeleton.Joints[JointType.ShoulderRight].Position);
				measuredArms.LeftElbowCms = Convert(validSkeleton.Joints[JointType.ElbowRight].Position);
				measuredArms.LeftHandCms = Convert(validSkeleton.Joints[JointType.HandRight].Position);
				engine.SetGender(WrapperCE.InterOp.UserGender.Male);
				WrapperCE.InterOp.ArmFatigueUpdate armFatigueUpdate = engine.ProcessNewSkeletonData(measuredArms, deltaTimeInSeconds);
			}

			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame != null)
				{
					colorFrame.CopyPixelDataTo(colorPixels);
					this.colorBitmap.WritePixels(
							new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight),
							colorPixels, colorBitmap.PixelWidth * sizeof(int),
							0);

					if (validSkeleton != null)
					{
						DrawingGroup dGroup = new DrawingGroup();
						using (DrawingContext dc = dGroup.Open())
						{
							dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Black, 0.5), new Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));
							skeletonDrawer.DrawSkeleton(validSkeleton, dc);
						}
						DrawingImage dImageSource = new DrawingImage(dGroup);
						dGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));
						iSkeleton.Source = dImageSource;
					}
					else
						iSkeleton.Source = null;
				}
			}
		}

		private WrapperCE.InterOp.Point3D Convert(SkeletonPoint trackedPoint)
		{
			WrapperCE.InterOp.Point3D point3D;
      point3D.X = trackedPoint.X;
      point3D.Y = trackedPoint.Y;
      point3D.Z = trackedPoint.Z;
      return point3D;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}
	}
}
