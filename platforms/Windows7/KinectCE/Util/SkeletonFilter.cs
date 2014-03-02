using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace CEWorkbench.Util
{

  public class SkeletonFilter
  {

    public Skeleton StableSkeleton { get; set; }

    private CircularList<Skeleton> SkeletonsBuffer { get; set; }

    public SkeletonFilter(int bufferSize)
    {
      SkeletonsBuffer = new CircularList<Skeleton>(bufferSize);
    }

    public Skeleton FilterSkeletonData(Skeleton newData)
    {
      //Process it and updates the StableSkeleton
      SkeletonsBuffer.Value = newData;
      SkeletonsBuffer.Next();

      //Calculates the average skeleton from all those in the circular list
      Skeleton stableSkeleton = new Skeleton();
      stableSkeleton.TrackingState = SkeletonTrackingState.Tracked;
      stableSkeleton.ClippedEdges = newData.ClippedEdges;
      stableSkeleton.TrackingId = newData.TrackingId;

      foreach (JointType joint in Enum.GetValues(typeof(JointType)))
      {
        Joint avgJoint = stableSkeleton.Joints[joint];
        avgJoint.TrackingState = JointTrackingState.Tracked;
        avgJoint.Position = GetAvgPosition(joint);

        stableSkeleton.Joints[joint] = avgJoint;
      }

      StableSkeleton = stableSkeleton;
      return StableSkeleton;
    }

    private SkeletonPoint GetAvgPosition(JointType joint)
    {
      float avgX = 0, avgY = 0, avgZ = 0;

			avgX = SkeletonsBuffer.Average(skeleton => skeleton.Joints[joint].Position.X);
			avgY = SkeletonsBuffer.Average(skeleton => skeleton.Joints[joint].Position.Y);
			avgZ = SkeletonsBuffer.Average(skeleton => skeleton.Joints[joint].Position.Z);

      return new SkeletonPoint() { X = avgX, Y = avgY, Z = avgZ };
    }

    public void Reset()
    {
      SkeletonsBuffer.Clear();
    }
  }

}
