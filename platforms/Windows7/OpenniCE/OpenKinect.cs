using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNIWrapper;
using System.Windows;
using NiTEWrapper;

namespace OpenNiCE
{
	public class OpenKinect
	{
		private Object colorMutex = new Object();

		private System.Windows.Media.Imaging.WriteableBitmap colorBitmap;
		private VideoFrameRef colorFrame;
		private Device kinectDevice;
		private VideoStream colorSensor;

		public UserTracker SkeletonSensor { get; set; }
		
		public double FrameWidth 
		{
			get { return colorFrame.FrameSize.Width; }
		}

		public double FrameHeight
		{
			get { return colorFrame.FrameSize.Height; }
		}

		public OpenKinect()
		{
			HandleOpenNIError(OpenNI.Initialize());
			DeviceInfo[] devices = OpenNI.EnumerateDevices();
			if (devices.Length == 0)
				HandleOpenNIError(OpenNI.Status.NoDevice);
			kinectDevice = devices[0].OpenDevice();

			//Start color Sensor
			colorSensor = kinectDevice.CreateVideoStream(Device.SensorType.Color);
			VideoMode[] videoModes = colorSensor.SensorInfo.GetSupportedVideoModes();
			colorSensor.VideoMode = videoModes[1];
			colorSensor.Start();
			colorSensor.OnNewFrame += new VideoStream.VideoStreamNewFrame(ColorSensor_OnNewFrame);

			//Start Skeleton Sensor
			HandleNiteError(NiTE.Initialize());
			try
			{
				SkeletonSensor = UserTracker.Create();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return;
			}
		}

		private void ColorSensor_OnNewFrame(VideoStream vStream)
		{
			if (!vStream.IsValid || !vStream.IsFrameAvailable())
				return;

			VideoFrameRef frame = vStream.ReadFrame();
			if (!frame.IsValid)
				return;

			lock (colorMutex)
			{
				colorFrame = frame;
			}

		}

		public System.Windows.Media.ImageSource RawImageSource
		{
			get
			{
				lock (colorMutex)
				{
					if (colorFrame == null || !colorFrame.IsValid)
						return null;

					var width = colorFrame.FrameSize.Width;
					var height = colorFrame.FrameSize.Height;
					if (colorBitmap == null)
						colorBitmap = new System.Windows.Media.Imaging.WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Rgb24, null);

					colorBitmap.Lock();
					colorBitmap.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), colorFrame.Data, colorFrame.DataSize, colorFrame.DataStrideBytes);
					colorBitmap.Unlock();

					colorFrame.Dispose();
					return colorBitmap;
				}
			}
		}

		private bool HandleOpenNIError(OpenNI.Status status)
		{
			if (status == OpenNI.Status.Ok)
				return true;
			MessageBox.Show("Error: " + status.ToString() + " - " + OpenNI.LastError, "Error");
			return false;
		}

		private bool HandleNiteError(NiTE.Status status)
		{
			if (status == NiTE.Status.Ok)
				return true;
			MessageBox.Show("Error: " + status.ToString() + " - " + OpenNI.LastError, "Error");
			return false;
		}

		public void Dispose()
		{
			OpenNI.Shutdown();
			NiTE.Shutdown();
		}

	}
}
