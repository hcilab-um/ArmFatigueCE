using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using NiTEWrapper;
using System.Windows.Media.Media3D;


namespace OpenNICE
{

  class SkeletonDrawer
  {
    private const float RENDER_WIDTH = 640.0f;
    private const float RENDER_HEIGHT = 480.0f;
		private const double JOINT_THINCKNESS = 3;

    private UserTracker userTracker;
    private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
    private readonly Brush inferredJointBrush = Brushes.Yellow;
		
		private static readonly Brush TorqueBrush = new SolidColorBrush(Color.FromRgb(255,0,0)); //this needs to be static for the next line to work ...?
		private readonly Pen TorquePen = new Pen(TorqueBrush, 0);

		public SkeletonDrawer(UserTracker userTracker)
    {
      this.userTracker = userTracker;
    }

    public void DrawSkeleton(Skeleton skeleton, DrawingContext drawingContext)
    {
      if (skeleton == null)
        return;

      // Render Torso
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.Head, SkeletonJoint.JointType.Neck);
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.LeftShoulder, SkeletonJoint.JointType.Neck);
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.RightShoulder, SkeletonJoint.JointType.Neck);

			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.RightShoulder, SkeletonJoint.JointType.Torso);
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.LeftShoulder, SkeletonJoint.JointType.Torso);

			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.Torso, SkeletonJoint.JointType.RightHip);
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.Torso, SkeletonJoint.JointType.LeftHip);
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.RightHip, SkeletonJoint.JointType.LeftHip);


      // Left Arm
      this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.LeftShoulder, SkeletonJoint.JointType.LeftElbow);
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.LeftElbow, SkeletonJoint.JointType.LeftHand);

      // Right Arm
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.RightShoulder, SkeletonJoint.JointType.RightElbow);
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.RightElbow, SkeletonJoint.JointType.RightHand);

      // Left Leg
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.LeftHip, SkeletonJoint.JointType.LeftKnee);
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.LeftKnee, SkeletonJoint.JointType.LeftFoot);

      // Right Leg
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.RightHip, SkeletonJoint.JointType.RightKnee);
			this.drawBone(skeleton, drawingContext, SkeletonJoint.JointType.RightKnee, SkeletonJoint.JointType.RightFoot);

			// Render Joints
			foreach (SkeletonJoint.JointType jointType in (SkeletonJoint.JointType[]) Enum.GetValues(typeof(SkeletonJoint.JointType)))
			{
				Brush drawBrush = null;
				SkeletonJoint joint = skeleton.GetJoint(jointType);
				if(!joint.IsValid)
					continue;
				drawBrush = this.trackedJointBrush;
				drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JOINT_THINCKNESS, JOINT_THINCKNESS);
			}

    }

		internal void DrawCirlce(Skeleton skeleton, SkeletonJoint.JointType jointType, DrawingContext dc, double radius)
		{
			if (skeleton == null)
				return;

			Point3D jointPoint = skeleton.GetJoint(jointType).Position;
			
			dc.DrawEllipse(TorqueBrush, TorquePen, SkeletonPointToScreen(jointPoint), radius, radius);

		}

    #region boneDrawing

    /// <summary>
    /// Pen used for drawing bones that are currently tracked
    /// </summary>
    private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

    /// <summary>
    /// Draws a bone line between two joints
    /// </summary>
    /// <param name="skeleton">skeleton to draw bones from</param>
    /// <param name="drawingContext">drawing context to draw to</param>
    /// <param name="jointType0">joint to start drawing from</param>
    /// <param name="jointType1">joint to end drawing at</param>
		private void drawBone(Skeleton skeleton, DrawingContext drawingContext, SkeletonJoint.JointType jointType0, SkeletonJoint.JointType jointType1)
    {
			if (skeleton.State != Skeleton.SkeletonState.Tracked)
				return;
			SkeletonJoint joint0 = skeleton.GetJoint(jointType0);
			SkeletonJoint joint1 = skeleton.GetJoint(jointType1);

      // If we can't find either of these joints, exit
      if (joint0.Position.Z <=0 || joint1.Position.Z <=0)
      {
        return;
      }

      // We assume all drawn bones are inferred unless BOTH joints are tracked
      Pen drawPen = this.trackedBonePen;

      drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
    }
    #endregion

    #region skeletonPoint to 2D point translation
		private const double _NUI_CAMERA_COLOR_NOMINAL_FOCAL_LENGTH_IN_PIXELS = 531.15;   // Based on 640x480 pixel size.
		private const double _FLT_EPSILON = 1.192092896e-07;
    /// <summary>
    /// Maps a SkeletonPoint to lie within our render space and converts to Point
    /// </summary>
    /// <param name="skelpoint">point to map</param>
    /// <returns>mapped point</returns>
    private Point SkeletonPointToScreen(Point3D skelpoint)
    {
      // Convert point to depth space.  
      // We are not using depth directly, but we do want the points in our 640x480 output resolution.
      System.Drawing.PointF depthPoint;
			depthPoint = userTracker.ConvertJointCoordinatesToDepth(skelpoint);
      return new Point(depthPoint.X, depthPoint.Y);
		}
		#endregion
	}
}
