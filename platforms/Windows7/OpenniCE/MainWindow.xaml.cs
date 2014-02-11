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
//using System.Drawing;

namespace OpenNICE
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private UserTracker userTracker;
		//private Bitmap image = new Bitmap(1, 1);

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			NiTE.Status status = NiTE.Initialize();

			if (status != NiTE.Status.Ok)
			{
				Console.WriteLine("Error: {0}", status);
				return;
			}
			try
			{
				this.userTracker = UserTracker.Create();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			userTracker.OnNewData += new UserTracker.UserTrackerListenerDelegate(userTracker_OnNewData);
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			NiTE.Shutdown();
			OpenNI.Shutdown();
		}

		private void userTracker_OnNewData(UserTracker userTracker)
		{
			if (!userTracker.IsValid)
				return;

			using (UserTrackerFrameRef frame = userTracker.ReadFrame())
			{
				if (frame == null || !frame.IsValid)
					return;
			}

			//lock (this.image)
			//{
			//  if (this.image.Width != frame.UserMap.FrameSize.Width
			//      || this.image.Height != frame.UserMap.FrameSize.Height)
			//  {
			//    this.image = new Bitmap(
			//        frame.UserMap.FrameSize.Width,
			//        frame.UserMap.FrameSize.Height,
			//        PixelFormat.Format24bppRgb);
			//  }

			//  using (Graphics g = Graphics.FromImage(this.image))
			//  {
			//    g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), this.image.Size));
			//    foreach (UserData user in frame.Users)
			//    {
			//      if (user.IsNew && user.IsVisible)
			//      {
			//        userTracker.StartSkeletonTracking(user.UserId);
			//      }

			//      if (user.IsVisible && user.Skeleton.State == Skeleton.SkeletonState.Tracked)
			//      {
			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.RightHand,
			//            SkeletonJoint.JointType.RightElbow);
			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.LeftHand,
			//            SkeletonJoint.JointType.LeftElbow);

			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.RightElbow,
			//            SkeletonJoint.JointType.RightShoulder);
			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.LeftElbow,
			//            SkeletonJoint.JointType.LeftShoulder);

			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.RightFoot,
			//            SkeletonJoint.JointType.RightKnee);
			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.LeftFoot,
			//            SkeletonJoint.JointType.LeftKnee);

			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.RightKnee,
			//            SkeletonJoint.JointType.RightHip);
			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.LeftKnee,
			//            SkeletonJoint.JointType.LeftHip);

			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.RightShoulder,
			//            SkeletonJoint.JointType.LeftShoulder);
			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.RightHip,
			//            SkeletonJoint.JointType.LeftHip);

			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.RightShoulder,
			//            SkeletonJoint.JointType.RightHip);
			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.LeftShoulder,
			//            SkeletonJoint.JointType.LeftHip);

			//        this.DrawLineBetweenJoints(
			//            g,
			//            user.Skeleton,
			//            SkeletonJoint.JointType.Head,
			//            SkeletonJoint.JointType.Neck);
			//      }
			//    }

			//    g.Save();
			//  }
			//}
		}

		//private void DrawLineBetweenJoints(
		//Graphics g,
		//Skeleton skel,
		//SkeletonJoint.JointType j1,
		//SkeletonJoint.JointType j2)
		//{
		//  try
		//  {
		//    if (skel.State == Skeleton.SkeletonState.Tracked)
		//    {
		//      SkeletonJoint joint1 = skel.GetJoint(j1);
		//      SkeletonJoint joint2 = skel.GetJoint(j2);
		//      if (joint1.Position.Z > 0 && joint2.Position.Z > 0)
		//      {
		//        Point joint1PosEllipse = new Point();
		//        Point joint2PosEllipse = new Point();
		//        PointF joint1PosLine = this.userTracker.ConvertJointCoordinatesToDepth(joint1.Position);
		//        PointF joint2PosLine = this.userTracker.ConvertJointCoordinatesToDepth(joint2.Position);
		//        joint1PosEllipse.X = (int)joint1PosLine.X - 5;
		//        joint1PosEllipse.Y = (int)joint1PosLine.Y - 5;
		//        joint2PosEllipse.X = (int)joint2PosLine.X - 5;
		//        joint2PosEllipse.Y = (int)joint2PosLine.Y - 5;
		//        joint1PosLine.X -= 2;
		//        joint1PosLine.Y -= 2;
		//        joint2PosLine.X -= 2;
		//        joint2PosLine.Y -= 2;
		//        g.DrawLine(new Pen(Brushes.White, 3), joint1PosLine, joint2PosLine);
		//        g.DrawEllipse(new Pen(Brushes.White, 5), new Rectangle(joint1PosEllipse, new Size(5, 5)));
		//        g.DrawEllipse(new Pen(Brushes.White, 5), new Rectangle(joint2PosEllipse, new Size(5, 5)));
		//      }
		//    }
		//  }
		//  catch (Exception)
		//  {
		//  }
		//}

	}
}
