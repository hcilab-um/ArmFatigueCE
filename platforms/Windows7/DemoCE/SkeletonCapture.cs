using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace DemoCE
{
  [Serializable]
  public class SkeletonCapture
  {
    public double DelayInMilliSeconds { get; set; }
    public Skeleton Skeleton { get; set; }

    [NonSerialized]
    public int FrameNro;
  }
}
