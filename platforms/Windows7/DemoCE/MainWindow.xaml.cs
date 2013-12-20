﻿using System;
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
using WrapperCE.InterOp;
using System.Collections.ObjectModel;
using DemoCE.Controls;

namespace DemoCE
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region Private Variable
		private KinectSensor kinectSensor = null;
		private WriteableBitmap colorBitmap;
		private byte[] colorPixels;
		private SkeletonDrawer skeletonDrawer;
		private long lastUpdate = -1;
		private bool isKinectConnected = false;

		private WrapperCE.EngineCE engine;

		private ArmFatigueUpdate armFatigueUpdate;

		private double deltaTimeInSeconds;
		private double totalTimeInSeconds;
		private FatigueInfo currentFatigueInfo;
		private bool playBackFromFile; 
		#endregion

		#region Property

		public SkeletonRecorder Recorder { get; set; }
		public SkeletonPlayer Player { get; set; }

		public ObservableCollection<FatigueInfo> FatigueInfoCollection { get; set; }

		public bool IsKinectConnected
		{
			get { return isKinectConnected; }
			set
			{
				isKinectConnected = value;
				OnPropertyChanged("IsKinectConnected");
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

		public double TotalTimeInSeconds
		{
			get { return totalTimeInSeconds; }
			set
			{
				totalTimeInSeconds = value;
				OnPropertyChanged("TotalTimeInSeconds");
			}
		}

		public FatigueInfo CurrentFatigueInfo 
		{
			get { return currentFatigueInfo; }
			set
			{
				currentFatigueInfo = value;
				OnPropertyChanged("CurrentFatigueInfo");
			}
		}

		public bool PlayBackFromFile
		{
			get { return playBackFromFile; }
			set
			{
				playBackFromFile = value;
				OnPropertyChanged("PlayBackFromFile");
			}
		}

		#endregion

		private EventHandler playbackHandler;
		public event EventHandler<ColorImageReadyArgs> ColorImageReady;
		public event PropertyChangedEventHandler PropertyChanged;

		public MainWindow()
		{
			engine = new WrapperCE.EngineCE();
			armFatigueUpdate = new ArmFatigueUpdate();
			CurrentFatigueInfo = new FatigueInfo();
			CurrentFatigueInfo.Gender = UserGender.Male;
			TotalTimeInSeconds = 0;
			FatigueInfoCollection = new ObservableCollection<DemoCE.FatigueInfo>();
			InitializeComponent();
		}

		private void DeleteFatigueInfo(object sender, RoutedEventArgs e)
		{
			TimelineControl tlControl = (TimelineControl)e.OriginalSource;
			FatigueInfoCollection.Remove(tlControl.FatigueInfo);
		}

		private void ReplayFatigue(object sender, RoutedEventArgs e)
		{
			TimelineControl tlControl = (TimelineControl)e.OriginalSource;
			engine.Start(tlControl.FatigueInfo.Gender);
			OnPropertyChanged("IsEngineRunning");
			PlayBack(tlControl.FatigueInfo.FatigueFileName, Player_PlaybackFinished, true);
		}

		public void Player_PlaybackFinished(object sender, EventArgs e)
		{
			PlayBackFromFile = false;
			if (engine.CheckStarted())
			{
				engine.Stop();
				OnPropertyChanged("IsEngineRunning");
			}
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

			RunFatigueEngine(skeleton, e.Delay);
			//We paint the skeleton and send the image over to the UI
			if (ColorImageReady != null)
			{
				DrawingImage imageCanvas = DrawSkeleton(skeleton, Brushes.Black);
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
			Recorder.Stop(false, true, UserGender.Male);
		}

		private void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			if (PlayBackFromFile)
				return;
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
				double deltaTimeMilliseconds = (currentTimeMilliseconds - lastUpdate);
				if (lastUpdate == -1)
					deltaTimeMilliseconds = 0;
				DeltaTimeInSeconds = deltaTimeMilliseconds / 1000;
				lastUpdate = currentTimeMilliseconds;
				RunFatigueEngine(validSkeleton, DeltaTimeInSeconds);
				Recorder.ProcessNewSkeletonData(validSkeleton, deltaTimeMilliseconds);
			}
			
			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame != null)
				{
					colorFrame.CopyPixelDataTo(colorPixels);
					this.colorBitmap.WritePixels(
							new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight),
							colorPixels, colorBitmap.PixelWidth * sizeof(int), 0);

					if (validSkeleton != null)
					{
						iSkeleton.Source = DrawSkeleton(validSkeleton, Brushes.Transparent);
					}
					else
						iSkeleton.Source = null;
				}
			}
		}

		private DrawingImage DrawSkeleton(Skeleton skeleton, SolidColorBrush brush)
		{
			DrawingGroup dGroup = new DrawingGroup();
			using (DrawingContext dc = dGroup.Open())
			{
				dc.DrawRectangle(brush, new Pen(Brushes.Black, 0.5), new Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));
				skeletonDrawer.DrawSkeleton(skeleton, dc);
			}
			DrawingImage dImageSource = new DrawingImage(dGroup);
			dGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));
			return dImageSource;
		}

		private void RunFatigueEngine(Skeleton skeleton, double deltaTimeInSeconds)
		{
			if (!engine.CheckStarted())
				return;
			TotalTimeInSeconds += deltaTimeInSeconds;
			SkeletonData measuredArms = new SkeletonData();
			measuredArms.RightShoulderCms = Convert(skeleton.Joints[JointType.ShoulderRight].Position);
			measuredArms.RightElbowCms = Convert(skeleton.Joints[JointType.ElbowRight].Position);
			measuredArms.RightHandCms = Convert(skeleton.Joints[JointType.HandRight].Position);
			measuredArms.LeftShoulderCms = Convert(skeleton.Joints[JointType.ShoulderLeft].Position);
			measuredArms.LeftElbowCms = Convert(skeleton.Joints[JointType.ElbowLeft].Position);
			measuredArms.LeftHandCms = Convert(skeleton.Joints[JointType.HandLeft].Position);
			armFatigueUpdate = engine.ProcessNewSkeletonData(measuredArms, deltaTimeInSeconds);

			CurrentFatigueInfo.LeftArmAngle = armFatigueUpdate.LeftArm.Theta;
			CurrentFatigueInfo.LeftArmAvgEndurance = armFatigueUpdate.LeftArm.AvgEndurance;
			CurrentFatigueInfo.LeftArmAvgTorque = armFatigueUpdate.LeftArm.AvgShoulderTorque;
			CurrentFatigueInfo.LeftArmConsumedEndurance = armFatigueUpdate.LeftArm.ConsumedEndurance;
			CurrentFatigueInfo.LeftArmTorque = armFatigueUpdate.LeftArm.ShoulderTorque;

			CurrentFatigueInfo.RightArmAngle = armFatigueUpdate.RightArm.Theta;
			CurrentFatigueInfo.RightArmAvgEndurance = armFatigueUpdate.RightArm.AvgEndurance;
			CurrentFatigueInfo.RightArmAvgTorque = armFatigueUpdate.RightArm.AvgShoulderTorque;
			CurrentFatigueInfo.RightArmConsumedEndurance = armFatigueUpdate.RightArm.ConsumedEndurance;
			CurrentFatigueInfo.RightArmTorque = armFatigueUpdate.RightArm.ShoulderTorque;
		}

		private void PlayBack(string fileName, EventHandler pbFinished, bool useDelay)
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

		private Point3D Convert(SkeletonPoint trackedPoint)
		{
			Point3D point3D;
      point3D.X = trackedPoint.X;
      point3D.Y = trackedPoint.Y;
      point3D.Z = trackedPoint.Z;
      return point3D;
		}

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		private void BtStartMeasure_Click(object sender, RoutedEventArgs e)
		{
			CurrentFatigueInfo = new FatigueInfo();
			FatigueInfoCollection.Insert(0, CurrentFatigueInfo);
			Recorder.Start();
			engine.Start(CurrentFatigueInfo.Gender);
			OnPropertyChanged("IsEngineRunning");
			TotalTimeInSeconds = 0;
			
		}

		private void BtStopMeasure_Click(object sender, RoutedEventArgs e)
		{
			CurrentFatigueInfo.FatigueFileName = Recorder.Stop(true, false, CurrentFatigueInfo.Gender);
			engine.Stop();
			if (CurrentFatigueInfo.FatigueFileName == string.Empty)
				FatigueInfoCollection.Remove(CurrentFatigueInfo);
			OnPropertyChanged("IsEngineRunning");
		}

	}
}
