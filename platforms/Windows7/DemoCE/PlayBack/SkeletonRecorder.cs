using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.ComponentModel;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Microsoft.Kinect;
using WrapperCE.InterOp;

namespace DemoCE.Playback
{

  public class SkeletonRecorder : INotifyPropertyChanged
  {

    public event PropertyChangedEventHandler PropertyChanged;

    private String folderPath = Environment.CurrentDirectory;
    private String tmpFileName = null;
    private BinaryFormatter formatter = null;
    private FileStream recordFile = null;
    private BinaryWriter writer = null;
    private bool isRecording = false;

    private double delta = 0;
    private double totalTime = 0;

    public bool IsRecording
    {
      get { return isRecording; }
      private set
      {
        isRecording = value;
        OnPropertyChanged("IsRecording");
        OnPropertyChanged("IsNotRecording");
      }
    }

    public bool IsNotRecording
    {
      get { return !isRecording; }
    }

    private int framesRecorded = 0;
    public int FramesRecorded
    {
      get { return framesRecorded; }
      set
      {
        framesRecorded = value;
        OnPropertyChanged("FramesRecorded");
      }
    }

    public double TotalTime
    {
      get { return totalTime; }
      set
      {
        totalTime = value;
        OnPropertyChanged("TotalTime");
      }
    }

    public double Delta
    {
      get { return delta; }
      set
      {
        delta = value;
        OnPropertyChanged("Delta");
      }
    }

    public SkeletonRecorder(String fPath)
    {
      folderPath = fPath;
      formatter = new BinaryFormatter();
    }

    public void ProcessNewSkeletonData(Skeleton skeleton, double deltaTimeMilliseconds)
    {
			if (!isRecording)
        return;

      Delta = deltaTimeMilliseconds / 1000.000;
      TotalTime += Delta;

      SkeletonCapture capture = new SkeletonCapture() { Delay = deltaTimeMilliseconds, Skeleton = skeleton };
      try
      {
        MemoryStream memTmp = new MemoryStream();
        formatter.Serialize(memTmp, capture);
        byte[] buffer = memTmp.GetBuffer();

        writer.Write(buffer.Length);
        writer.Write(buffer, 0, (int)buffer.Length);
        FramesRecorded++;
      }
      catch (SerializationException e)
      {
        Console.WriteLine("Failed to serialize. Reason: " + e.Message);
        throw;
      }
    }

    public void Start()
    {
      if (isRecording)
        throw new Exception("The system is already recording, therefore cannot be started");

      tmpFileName = System.IO.Path.GetTempFileName().Replace(".tmp", ".kr");
      recordFile = File.Open(tmpFileName, FileMode.CreateNew, FileAccess.ReadWrite);
      writer = new BinaryWriter(recordFile);

      IsRecording = true;
      Delta = 0;
      TotalTime = 0;
    }

		public String Stop(bool saveFile, bool shutdown, UserGender gender)
    {
      if (!isRecording)
      {
        if (shutdown)
          return String.Empty;
        throw new Exception("The system is not recording, therefore cannot be stopped");
      }

      IsRecording = false;
      writer.Flush();
      writer.Close();

      if (saveFile && FramesRecorded != 0)
      {
        String qualifiedName = String.Format("{0}-{1}",
					DateTime.Now.ToString("MMddyy-HHmmss"), gender.ToString().ToLower());
        String newFileName = folderPath + @"\" + qualifiedName + ".kr";
        File.Move(tmpFileName, newFileName);
        return newFileName;
      }
      else
      {
        File.Delete(tmpFileName);
        return String.Empty;
      }
    }

    private void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }

  }

}
