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
using DemoCE.Playback;
using DemoCE.Properties;

namespace DemoCE
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public event EventHandler<ColorImageReadyArgs> ColorImageReady;

		#region Private Variable

		private KinectSensor kinectSensor = null;
		private WriteableBitmap colorBitmap;
		private byte[] colorPixels;
		private SkeletonDrawer skeletonDrawer;
		private long lastUpdate = -1;
		private bool isKinectConnected = false;
		private WrapperCE.EngineCE engine;
		private WrapperCE.InterOp.UserGender gender;
		private WrapperCE.InterOp.ArmFatigueUpdate armFatigueUpdate;
		private double consumedEndurance;
		private double deltaTimeInSeconds;	
		
		#endregion

		#region Property
		
		public SkeletonRecorder Recorder { get; set; }
		public SkeletonPlayer Player { get; set; }
		
		public bool PlayBackFromFile { get; set; }

		public bool IsKinectConnected
		{
			get { return isKinectConnected; }
			set
			{
				isKinectConnected = value;
				OnPropertyChanged("IsKinectConnected");
			}
		}

		public WrapperCE.InterOp.UserGender Gender
		{
			get { return gender; }
			set
			{
				gender = value;
				OnPropertyChanged("Gender");
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
		
		public bool IsEngineRunning
		{
			get { return engine.CheckStarted(); }
		}

		public double DeltaTimeInSeconds
		{
			get { return deltaTimeInSeconds; }
			set
			{
				deltaTimeInSeconds = value;
				OnPropertyChanged("DeltaTimeInSeconds");
			}

		}
		#endregion

		public MainWindow()
		{
			engine = new WrapperCE.EngineCE();
			Gender = WrapperCE.InterOp.UserGender.Male;
			armFatigueUpdate = new WrapperCE.InterOp.ArmFatigueUpdate();
			ConsumedEndurance = 0;
			DeltaTimeInSeconds = 0;
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			Recorder = new SkeletonRecorder(Settings.Default.RecordFolder);
			Player = new SkeletonPlayer(Settings.Default.PlayerBufferSize, Dispatcher);
			Player.SkeletonFrameReady += new EventHandler<PlayerSkeletonFrameReadyEventArgs>(player_SkeletonFrameReady);
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
			this.ColorImageReady += MainWindow_ColorImageReady;
		}

		private void MainWindow_ColorImageReady(object sender, ColorImageReadyArgs e)
		{
 			ImageSource colorFrame = e.Frame;
			if (colorFrame == null)
				return;
			iSkeleton.Source = colorFrame;
		}

		private void player_SkeletonFrameReady(object sender, PlayerSkeletonFrameReadyEventArgs e)
		{
			Skeleton skeleton = e.FrameSkeleton;
			DeltaTimeInSeconds = e.Delay;
			RunFatigueEngine(skeleton, DeltaTimeInSeconds);
			//We paint the skeleton and send the image over to the UI
			if (ColorImageReady != null)
			{
				DrawingImage imageCanvas = DrawSkeleton(skeleton, null);
				ColorImageReady(this, new ColorImageReadyArgs() { Frame = imageCanvas });
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if (null != kinectSensor)
			{
				kinectSensor.Stop();
				kinectSensor.Dispose();
			}
			this.ColorImageReady -= MainWindow_ColorImageReady;
			Recorder.Stop(false, true, Gender);
		}

		private void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
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
				DeltaTimeInSeconds = (double)(currentTimeMilliseconds - lastUpdate)/1000;
				if (lastUpdate == -1)
					DeltaTimeInSeconds = 0;
				lastUpdate = currentTimeMilliseconds;
				RunFatigueEngine(validSkeleton, DeltaTimeInSeconds);
				Recorder.ProcessNewSkeletonData(validSkeleton, DeltaTimeInSeconds);
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
						iSkeleton.Source = DrawSkeleton(validSkeleton, colorBitmap);
					}
					else
						iSkeleton.Source = null;
				}
			}
		}

		private DrawingImage DrawSkeleton(Skeleton skeleton, WriteableBitmap bitMap)
		{
			DrawingGroup dGroup = new DrawingGroup();
			using (DrawingContext dc = dGroup.Open())
			{
				dc.DrawRectangle(Brushes.Transparent, new Pen(Brushes.Black, 0.5), new Rect(0, 0, bitMap.PixelWidth, bitMap.PixelHeight));
				skeletonDrawer.DrawSkeleton(skeleton, dc);
			}
			DrawingImage dImageSource = new DrawingImage(dGroup);
			dGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, bitMap.PixelWidth, bitMap.PixelHeight));
			return dImageSource;
		}

		private void RunFatigueEngine(Skeleton skeleton, double deltaTime)
		{
			if (!engine.CheckStarted())
				return;
			WrapperCE.InterOp.SkeletonData measuredArms = new WrapperCE.InterOp.SkeletonData();
			measuredArms.RightShoulderCms = Convert(skeleton.Joints[JointType.ShoulderRight].Position);
			measuredArms.RightElbowCms = Convert(skeleton.Joints[JointType.ElbowRight].Position);
			measuredArms.RightHandCms = Convert(skeleton.Joints[JointType.HandRight].Position);
			measuredArms.LeftShoulderCms = Convert(skeleton.Joints[JointType.ShoulderLeft].Position);
			measuredArms.LeftElbowCms = Convert(skeleton.Joints[JointType.ElbowLeft].Position);
			measuredArms.LeftHandCms = Convert(skeleton.Joints[JointType.HandLeft].Position);
			armFatigueUpdate = engine.ProcessNewSkeletonData(measuredArms, deltaTime);
			ConsumedEndurance = armFatigueUpdate.RightArm.ConsumedEndurance;
		}

		private EventHandler playbackHandler;

		public void PlayBack(string fileName, EventHandler pbFinished, bool useDelay)
		{
			if (playbackHandler != null)
				Player.PlaybackFinished -= playbackHandler;
			playbackHandler = pbFinished;
			Player.UseDelay = useDelay;
			Player.PlaybackFinished += playbackHandler;
			Player.Load(fileName);
			Player.Start();

			PlayBackFromFile = true;
		}

		private void MainW_ColorImageReady(object sender, ColorImageReadyArgs e)
		{
			ImageSource colorFrame = e.Frame;
			if (colorFrame == null)
				return;

			iSkeleton.Source = colorFrame;
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

		private void BtStartMeasure_Click(object sender, RoutedEventArgs e)
		{
			Recorder.Start();
			engine.Start(Gender);
			OnPropertyChanged("IsEngineRunning");
		}

		private void BtStopMeasure_Click(object sender, RoutedEventArgs e)
		{
			Recorder.Stop(true, false, Gender);
			engine.Stop();
			OnPropertyChanged("IsEngineRunning");
		}

	}
}
