using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Microsoft.Kinect;
using System.Windows;
using System.Diagnostics;


namespace KinectCE
{

  class SkeletonDrawer
  {
    private const float RENDER_WIDTH = 640.0f;
    private const float RENDER_HEIGHT = 480.0f;
		private const double JOINT_THINCKNESS = 3;

    private KinectSensor kinectSensor;
    private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
    private readonly Brush inferredJointBrush = Brushes.Yellow;
    
		
		private static readonly Brush TorqueBrush = new SolidColorBrush(Color.FromRgb(255,0,0)); //this needs to be static for the next line to work ...?
		private readonly Pen TorquePen = new Pen(TorqueBrush, 0);

    public SkeletonDrawer(KinectSensor kinectSensor)
    {
      this.kinectSensor = kinectSensor;
    }

    public void DrawSkeleton(Skeleton skeleton, DrawingContext drawingContext)
    {
      if (skeleton == null)
        return;

      // Render Torso
      this.drawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
      this.drawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
      this.drawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
      this.drawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
      this.drawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
      this.drawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
      this.drawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);

      // Left Arm
      this.drawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft);
      this.drawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft);
      this.drawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft);

      // Right Arm
      this.drawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight);
      this.drawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight);
      this.drawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight);

      // Left Leg
      this.drawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft);
      this.drawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft);
      this.drawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft);

      // Right Leg
      this.drawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight);
      this.drawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight);
      this.drawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight);

      // Render Joints
      foreach (Joint joint in skeleton.Joints)
      {
        Brush drawBrush = null;

        if (joint.TrackingState == JointTrackingState.Tracked)
        {
          drawBrush = this.trackedJointBrush;
        }
        else if (joint.TrackingState == JointTrackingState.Inferred)
        {
          drawBrush = this.inferredJointBrush;
        }

        if (drawBrush != null)
        {
          drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JOINT_THINCKNESS, JOINT_THINCKNESS);
        }
      }
    }

		internal void DrawCirlce(Skeleton skeleton, JointType jointType, DrawingContext dc, double radius)
		{
			if (skeleton == null)
				return;

			SkeletonPoint shoulder = skeleton.Joints[jointType].Position;
			
			dc.DrawEllipse(TorqueBrush, TorquePen, SkeletonPointToScreen(shoulder), radius, radius);

		}

    #region boneDrawing

    /// <summary>
    /// Pen used for drawing bones that are currently tracked
    /// </summary>
    private readonly Pen trackedBonePen = new Pen(Brushes.Green, 6);

    /// <summary>
    /// Pen used for drawing bones that are currently inferred
    /// </summary>        
    private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

    /// <summary>
    /// Draws a bone line between two joints
    /// </summary>
    /// <param name="skeleton">skeleton to draw bones from</param>
    /// <param name="drawingContext">drawing context to draw to</param>
    /// <param name="jointType0">joint to start drawing from</param>
    /// <param name="jointType1">joint to end drawing at</param>
    private void drawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
    {
      Joint joint0 = skeleton.Joints[jointType0];
      Joint joint1 = skeleton.Joints[jointType1];

      // If we can't find either of these joints, exit
      if (joint0.TrackingState == JointTrackingState.NotTracked ||
          joint1.TrackingState == JointTrackingState.NotTracked)
      {
        return;
      }

      // Don't draw if both points are inferred
      if (joint0.TrackingState == JointTrackingState.Inferred &&
          joint1.TrackingState == JointTrackingState.Inferred)
      {
        return;
      }

      // We assume all drawn bones are inferred unless BOTH joints are tracked
      Pen drawPen = this.inferredBonePen;
      if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
      {
        drawPen = this.trackedBonePen;
      }

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
    private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
    {
      // Convert point to depth space.  
      // We are not using depth directly, but we do want the points in our 640x480 output resolution.
      DepthImagePoint depthPoint;
      if (kinectSensor != null)
        depthPoint = kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
      else
        depthPoint = MapSkeletonPointToDepth(skelpoint, DepthImageFormat.Resolution640x480Fps30);
      return new Point(depthPoint.X, depthPoint.Y);
    }

    private DepthImagePoint MapSkeletonPointToDepth(SkeletonPoint skelpoint, DepthImageFormat depthImageFormat)
    {
      DepthImagePoint rPoint = new DepthImagePoint();
      rPoint.X = 0;
      rPoint.Y = 0;

      if (skelpoint.Z > _FLT_EPSILON)
      {
        rPoint.X = (int)(0.5 + skelpoint.X * (_NUI_CAMERA_COLOR_NOMINAL_FOCAL_LENGTH_IN_PIXELS / skelpoint.Z) / 640.0);
        rPoint.Y = (int)(0.5 - skelpoint.Y * (_NUI_CAMERA_COLOR_NOMINAL_FOCAL_LENGTH_IN_PIXELS / skelpoint.Z) / 480.0);
        return rPoint;
      }

      return rPoint;
    }
    #endregion

  }
}
