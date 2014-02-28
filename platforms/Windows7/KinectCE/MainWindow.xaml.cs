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
using KinectCE.Playback;
using KinectCE.Properties;
using WrapperCE.InterOp;
using System.Collections.ObjectModel;
using KinectCE.Controls;
using System.IO;
using log4net.Appender;
using log4net.Config;
using log4net;
using KinectCE.Fatigue;
using KinectCE.Util;

namespace KinectCE
{
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(typeof(MainWindow));

		#region Private Variable
		private const double TORQUE_MODIFIER = 2d;
		private KinectSensor kinectSensor = null;
		private SkeletonRecorder recorder;
		private SkeletonPlayer player;
		private WriteableBitmap colorBitmap;
		private byte[] colorPixels;
		private SkeletonDrawer skeletonDrawer;
		private long lastUpdate = -1;
		private bool isKinectConnected = false;

		private WrapperCE.EngineCE engine;

		private ArmFatigueUpdate armFatigueUpdate;

		private double deltaTimeInSeconds;
		private FatigueInfo currentFatigueInfo;
		private bool playBackFromFile;
		private bool isAutoStart;

		private string recordPath;
		private Arm arm;
		private UserGender gender;
		#endregion

		#region Property
		
		public SettingWindow SettingW { get; set; }

		public ObservableCollection<FatigueInfo> FatigueInfoCollection { get; set; }
		
		public SkeletonFilter SkeletonFilter { get; set; }
		public DoubleFilter DoubleFilter { get; set; }

		public bool IsKinectConnected
		{
			get { return isKinectConnected; }
			set
			{
				isKinectConnected = value;
				OnPropertyChanged("IsKinectConnected");
			}
		}

		public SkeletonRecorder Recorder
		{
			get { return recorder; }
			set
			{
				recorder = value;
				OnPropertyChanged("Recorder");
			}
		}

		public SkeletonPlayer Player
		{
			get { return player; }
			set
			{
				player = value;
				OnPropertyChanged("Player");
			}
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

		public bool IsAutoStart
		{
			get { return isAutoStart; }
			set
			{
				isAutoStart = value;
				OnPropertyChanged("IsAutoStart");
			}
		}

		public string RecordPath
		{
			get { return recordPath; }
			set
			{
				recordPath = value;
				OnPropertyChanged("RecordPath");
			}
		}

		public Arm Arm
		{
			get { return arm; }
			set
			{
				arm = value;
				OnPropertyChanged("Arm");
			}
		}

		public UserGender Gender
		{
			get { return gender; }
			set
			{
				gender = value;
				OnPropertyChanged("Gender");
			}
		}
		#endregion

		private EventHandler playbackHandler;
		public event EventHandler<ColorImageReadyArgs> ColorImageReady;
		public event PropertyChangedEventHandler PropertyChanged;

		public MainWindow()
		{
			RecordPath = Environment.CurrentDirectory;
			Gender = UserGender.Male;
			Arm = Arm.RightArm;
			IsAutoStart = false;
			FatigueInfoCollection = new ObservableCollection<FatigueInfo>();
			SkeletonFilter = new SkeletonFilter(Settings.Default.SkeletonBufferSize);
			DoubleFilter = new DoubleFilter(Settings.Default.StrengthBufferSize);
			InitializeComponent();
		}

		private void DeleteFatigueInfo(object sender, RoutedEventArgs e)
		{
			TimelineControl tlControl = (TimelineControl)e.OriginalSource;
			FatigueInfo fatigueInfo = (FatigueInfo)tlControl.DataContext;
			File.Delete(fatigueInfo.FatigueFile);
			FatigueInfoCollection.Remove(fatigueInfo);
		}

		private void ReplayFatigue(object sender, RoutedEventArgs e)
		{
			TimelineControl tlControl = (TimelineControl)e.OriginalSource;
			CurrentFatigueInfo = (FatigueInfo)tlControl.DataContext;
			CurrentFatigueInfo.Reset();
			engine.SetGender(CurrentFatigueInfo.Gender);
			engine.Reset();
			SkeletonFilter.Reset();
			PlayBack(CurrentFatigueInfo.FatigueFile, Player_PlaybackFinished, true);
		}

		public void Player_PlaybackFinished(object sender, EventArgs e)
		{
			PlayBackFromFile = false;
			CurrentFatigueInfo = new FatigueInfo() { Gender = Gender, SelectedArm = Arm };
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			engine = new WrapperCE.EngineCE();
			Player = new SkeletonPlayer(Settings.Default.PlayerBufferSize, Dispatcher);
			Recorder = new SkeletonRecorder(RecordPath);
			Player.SkeletonFrameReady += new EventHandler<PlayerSkeletonFrameReadyEventArgs>(player_SkeletonFrameReady);
			CurrentFatigueInfo = new FatigueInfo();
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
			Skeleton stableSkeleton = SkeletonFilter.FilterSkeletonData(e.FrameSkeleton);

			RunFatigueEngine(stableSkeleton, e.DelayInMilliSeconds / 1000);
			if (ColorImageReady != null)
			{
				DrawingImage imageCanvas = DrawSkeleton(stableSkeleton, Brushes.Black);
				ColorImageReady(this, new ColorImageReadyArgs() { Frame = imageCanvas });
			}
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			engine.Dispose();
			if (null != kinectSensor)
			{
				kinectSensor.Stop();
				kinectSensor.Dispose();
			}

			this.ColorImageReady -= MainWindow_ColorImageReady;

			Recorder.Stop(false, true, "", UserGender.Male);
		}

		private void kinectSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			if (PlayBackFromFile)
				return;

			long currentTimeMilliseconds = -1;
			Skeleton[] skeletons = null;
			Skeleton stableSkeleton = null;
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
						stableSkeleton = SkeletonFilter.FilterSkeletonData(skeleton);
						break;
					}
				}
			}

			if (stableSkeleton != null)
			{
				double deltaTimeMilliseconds = (currentTimeMilliseconds - lastUpdate);
				if (lastUpdate == -1)
					deltaTimeMilliseconds = 0;
				DeltaTimeInSeconds = deltaTimeMilliseconds / 1000;
				lastUpdate = currentTimeMilliseconds;
				RunFatigueEngine(stableSkeleton, DeltaTimeInSeconds);
				if (IsAutoStart)
				{
					if (Arm == Arm.LeftArm)
						AutoMeasure(CurrentFatigueInfo.LeftData);
					else
						AutoMeasure(CurrentFatigueInfo.RightData);
				}
				Recorder.ProcessNewSkeletonData(stableSkeleton, deltaTimeMilliseconds);
			}

			using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
			{
				if (colorFrame != null)
				{
					colorFrame.CopyPixelDataTo(colorPixels);
					this.colorBitmap.WritePixels(
							new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight),
							colorPixels, colorBitmap.PixelWidth * sizeof(int), 0);

					if (stableSkeleton != null)
					{
						iSkeleton.Source = DrawSkeleton(stableSkeleton, Brushes.Transparent);
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
				if (CurrentFatigueInfo.SelectedArm == Arm.RightArm)
					skeletonDrawer.DrawCirlce(skeleton, JointType.ShoulderRight, dc, CurrentFatigueInfo.RightData.ArmStrength / TORQUE_MODIFIER);
				else
					skeletonDrawer.DrawCirlce(skeleton, JointType.ShoulderLeft, dc, CurrentFatigueInfo.LeftData.ArmStrength / TORQUE_MODIFIER);
			}

			DrawingImage dImageSource = new DrawingImage(dGroup);
			dGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));
			return dImageSource;
		}

		private void RunFatigueEngine(Skeleton skeleton, double deltaTimeInSeconds)
		{
			CurrentFatigueInfo.TotalTimeInSeconds += deltaTimeInSeconds;
			SkeletonData measuredArms = new SkeletonData();
			measuredArms.RightShoulderCms = Convert(skeleton.Joints[JointType.ShoulderRight].Position);
			measuredArms.RightElbowCms = Convert(skeleton.Joints[JointType.ElbowRight].Position);
			measuredArms.RightWristCms = Convert(skeleton.Joints[JointType.WristRight].Position);
			measuredArms.RightHandCms = Convert(skeleton.Joints[JointType.HandRight].Position);

			measuredArms.LeftShoulderCms = Convert(skeleton.Joints[JointType.ShoulderLeft].Position);
			measuredArms.LeftElbowCms = Convert(skeleton.Joints[JointType.ElbowLeft].Position);
			measuredArms.LeftWristCms = Convert(skeleton.Joints[JointType.WristLeft].Position);
			measuredArms.LeftHandCms = Convert(skeleton.Joints[JointType.HandLeft].Position);

			armFatigueUpdate = engine.ProcessNewSkeletonData(measuredArms, deltaTimeInSeconds);

			LoadArmData(CurrentFatigueInfo.LeftData, armFatigueUpdate.LeftArm);
			LoadArmData(CurrentFatigueInfo.RightData, armFatigueUpdate.RightArm);
		}

		private void LoadArmData(ArmData arm, FatigueData data)
		{
			arm.Angle = data.Theta;
			arm.AvgEndurance = data.AvgEndurance;
			arm.AvgShoulderTorque = data.AvgShoulderTorque;
			arm.ConsumedEndurance = data.ConsumedEndurance;
			arm.ShoulderTorque = data.ShoulderTorque;
			arm.AvgShoulderTorque = data.AvgShoulderTorque;
			arm.ArmStrength = data.ArmStrength;
			arm.AvgArmStrength = data.AvgArmStrength;
		}

		private void AutoMeasure(ArmData data)
		{
			double triggerStrength = DoubleFilter.FilterData(data.ArmStrength);
			if (!Recorder.IsRecording && triggerStrength >= Settings.Default.AutoStartThreshold)
				BtStartMeasure_Click(null, null);
			else if (Recorder.IsRecording && triggerStrength < Settings.Default.AutoStartThreshold)
				BtStopMeasure_Click(null, null);
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

		private Point3D Convert(SkeletonPoint trackedPoint)
		{
			Point3D point3D;
			point3D.X = trackedPoint.X;
			point3D.Y = trackedPoint.Y;
			point3D.Z = trackedPoint.Z;
			return point3D;
		}

		private void BtStartMeasure_Click(object sender, RoutedEventArgs e)
		{
			if (PlayBackFromFile)
				return;
			Recorder = new SkeletonRecorder(RecordPath);
			SkeletonFilter.Reset();
			engine.Reset();
			CurrentFatigueInfo = new FatigueInfo() { DateTime = DateTime.Now, Gender = Gender, SelectedArm = Arm };
			FatigueInfoCollection.Insert(0, CurrentFatigueInfo);
			Recorder.Start();
		}

		private void BtStopMeasure_Click(object sender, RoutedEventArgs e)
		{
			if (PlayBackFromFile)
				return;

			String qualifiedName = String.Format("{0}-{1}-{2}", CurrentFatigueInfo.DateTime.ToString("MMddyy-HHmmss-fff"),
																						CurrentFatigueInfo.Gender.ToString().ToLower(), currentFatigueInfo.SelectedArm);
			CurrentFatigueInfo.FatigueName = qualifiedName;
			CurrentFatigueInfo.FatigueFile = Recorder.Stop(true, false, CurrentFatigueInfo.FatigueName, CurrentFatigueInfo.Gender);
			if (CurrentFatigueInfo.FatigueFile == string.Empty)
				FatigueInfoCollection.Remove(CurrentFatigueInfo);
			CurrentFatigueInfo = new FatigueInfo();
		}

		private IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
		{
			if (depObj != null)
			{
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
					if (child != null && child is T)
					{
						yield return (T)child;
					}

					foreach (T childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}

		private void BtExport_Click(object sender, RoutedEventArgs e)
		{
			var timeLineList = FindVisualChildren<TimelineControl>(lbTimeLines).ToList();

			if (timeLineList.Count == 0)
			{
				MessageBox.Show("No Fatigue Data for Export.");
				return;
			}

			XmlConfigurator.Configure();
			var hierarchy = (log4net.Repository.Hierarchy.Hierarchy)LogManager.GetRepository();
			RollingFileAppender appender = (RollingFileAppender)hierarchy.Root.Appenders[0];

			for (int i = 0; i < timeLineList.Count; i++)
				logger.Info(timeLineList[i].GetEffortLog());
			Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog();
			dialog.InitialDirectory = Environment.CurrentDirectory;
			dialog.DefaultExt = ".csv";
			dialog.Filter = "Text documents (.csv)|*.csv";

			Nullable<bool> result = dialog.ShowDialog();

			if (result == true)
			{
				string filename = dialog.FileName;
				File.Delete(filename);
				File.Move(appender.File, filename);
			}
			else
				File.Delete(appender.File);
		}

		private void BtSetting_Click(object sender, RoutedEventArgs e)
		{
			SettingW = new SettingWindow() { RecordPath = RecordPath, Gender = Gender, Arm = Arm };
			SettingW.ShowDialog();
			Gender = SettingW.Gender;
			Arm = SettingW.Arm;
			RecordPath = SettingW.RecordPath;
			engine.SetGender(Gender);
		}

		private void OnPropertyChanged(String name)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

	}
}
