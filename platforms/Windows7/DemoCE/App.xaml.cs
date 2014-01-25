using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using log4net.Appender;
using System.IO;

namespace DemoCE
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
		
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);
			log4net.Config.XmlConfigurator.Configure();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);
			RollingFileAppender fileAppender = log4net.LogManager.GetRepository().GetAppenders().First(appender => appender is RollingFileAppender) as RollingFileAppender;

			if (fileAppender != null && File.Exists(fileAppender.File))
			{
				File.Delete(fileAppender.File);
			}
		}
  }
}
