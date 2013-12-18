using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace DemoCE.Playback
{
  public class PlayerSkeletonFrameReadyEventArgs : EventArgs
  {
    public double Delay { get; set; }
    public Skeleton FrameSkeleton { get; set; }
  }
}
