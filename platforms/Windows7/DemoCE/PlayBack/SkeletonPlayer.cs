using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Kinect;
using System.ComponentModel;
using System.Windows.Threading;

namespace DemoCE.Playback
{

  public class SkeletonPlayer : INotifyPropertyChanged
  {
    public event PropertyChangedEventHandler PropertyChanged;
    public event EventHandler PlaybackFinished;
    public event EventHandler<PlayerSkeletonFrameReadyEventArgs> SkeletonFrameReady;

    private FileStream recordFile = null;
    private BinaryReader reader = null;
    private BinaryFormatter formatter = null;

    private BackgroundWorker fileLoaderBGW = null;
    private BackgroundWorker pumperBGW = null;
    private Queue<SkeletonCapture> loadedCaptures = null;

    private Dispatcher uiDispatcher = null;
    private bool isPlaying = false;
    private bool isDelay = false;
    public bool IsPlaying
    {
      get { return isPlaying; }
      private set
      {
        isPlaying = value;
        OnPropertyChanged("IsPlaying");
        OnPropertyChanged("IsNotPlaying");
      }
    }

    public bool UseDelay
    {
      get { return isDelay; }
      set
      {
        isDelay = value;
        OnPropertyChanged("IsDelay");
      }
    }

    public bool IsNotPlaying
    {
      get { return !isPlaying; }
    }

    public SkeletonPlayer(int qSize, Dispatcher dispatcher)
    {
      uiDispatcher = dispatcher;

      fileLoaderBGW = new BackgroundWorker();
      fileLoaderBGW.DoWork += new DoWorkEventHandler(fileLoaderBGW_DoWork);
      fileLoaderBGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(fileLoaderBGW_RunWorkerCompleted);

      pumperBGW = new BackgroundWorker();
      pumperBGW.DoWork += new DoWorkEventHandler(pumperBGW_DoWork);
      pumperBGW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(pumperBGW_RunWorkerCompleted);

      formatter = new BinaryFormatter();
      loadedCaptures = new Queue<SkeletonCapture>(qSize);
      isPlaying = false;
    }

    void pumperBGW_DoWork(object sender, DoWorkEventArgs e)
    {
      if (SkeletonFrameReady == null)
        return;

      foreach (SkeletonCapture capture in loadedCaptures)
      {
        if(UseDelay)
          System.Threading.Thread.Sleep((int)capture.Delay);
        if (uiDispatcher != null)
        {
          uiDispatcher.Invoke((Action)delegate
          {
            SkeletonFrameReady(this, new PlayerSkeletonFrameReadyEventArgs() { Delay = capture.Delay, FrameSkeleton = capture.Skeleton });
          }, null);
        }
        if (uiDispatcher == null)
        {
          SkeletonFrameReady(this, new PlayerSkeletonFrameReadyEventArgs() { Delay = capture.Delay, FrameSkeleton = capture.Skeleton });
        }
      }
    }

    void pumperBGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      if (PlaybackFinished != null)
      {
        PlaybackFinished(this, new EventArgs());
        IsPlaying = false;
      }
    }

    void fileLoaderBGW_DoWork(object sender, DoWorkEventArgs e)
    {
      loadedCaptures.Clear();

      int objectLenght;
      do
      {
        objectLenght = reader.ReadInt32();
        byte[] buffer = new byte[objectLenght];
        reader.Read(buffer, 0, objectLenght);
        MemoryStream stream = new MemoryStream(buffer);
        SkeletonCapture capture = (SkeletonCapture)formatter.Deserialize(stream);
        loadedCaptures.Enqueue(capture);
      } while (reader.BaseStream.Position < reader.BaseStream.Length);
    }

    void fileLoaderBGW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      pumperBGW.RunWorkerAsync();
    }

    public void Load(string filePath)
    {
      if (!File.Exists(filePath))
        throw new Exception(String.Format("File {0} does not exists", filePath));

      recordFile = new FileStream(filePath, FileMode.Open, FileAccess.Read);
      reader = new BinaryReader(recordFile);

      if (recordFile.Length == 0)
        throw new Exception(String.Format("File {0} is empty", filePath));

      int objectLenght = reader.ReadInt32();
      if (objectLenght <= 0)
        throw new Exception(String.Format("The file does not seem to have the right format - objectLenght: {0}", objectLenght));

      byte[] buffer = new byte[objectLenght];
      reader.Read(buffer, 0, objectLenght);
      MemoryStream stream = new MemoryStream(buffer);
      SkeletonCapture capture = (SkeletonCapture)formatter.Deserialize(stream);

      recordFile.Seek(0, SeekOrigin.Begin);
    }

    /// <summary>
    /// Loads 
    /// </summary>
    public void Start()
    {
      IsPlaying = true;
      fileLoaderBGW.RunWorkerAsync();
    }

    private void OnPropertyChanged(String name)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(name));
    }
  }

}
