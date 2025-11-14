using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Architexor.Core
{
	public enum LogLevel
	{
		INFO = 0x01,
		WARNING,
		ERROR
	}

	public class TraceLogger
	{
		private string _logFilePath = "";
		private static readonly long _maxLogFileSizeInBytes = 10 * 1024 * 1024; // 10 MB

		private static TraceLogger _instance;

		public static TraceLogger Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = new TraceLogger();
				}

				return _instance;
			}
		}

		public TraceLogger() 
		{
			string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			//string dbPath = Path.Combine(url.Substring(0, url.LastIndexOf("\\")) + "\\", "IntelliBIM.db");
			string dbPath = Path.Combine(appDataFolder, "IntelliBIM", "ExceptionLogger.txt");
			// Ensure the directory exists
			Directory.CreateDirectory(Path.GetDirectoryName(dbPath));

			_logFilePath = dbPath;
		}

		public void ExceptionLog(string comment, Exception ex = null, LogLevel level = LogLevel.ERROR)
		{
			try
			{
				CheckAndRotateLogFile();

				// Ensure the log file exists
				if (!File.Exists(_logFilePath)) { }
				Console.WriteLine("Log file created: " + _logFilePath);

				StringBuilder sb = new StringBuilder();
				if(ex == null)
				{
					sb.AppendLine("===== Exception Log ======");
					sb.AppendLine($"Timestamp: {DateTime.Now}");
					sb.AppendLine(comment);
					sb.AppendLine("============================");
					sb.AppendLine();
				}
				else
				{
					sb.AppendLine("===== Exception Log ======");
					sb.AppendLine($"Timestamp: {DateTime.Now}");
					sb.AppendLine(comment);
					sb.AppendLine($"Message: {ex.Message}");
					sb.AppendLine($"Source: {ex.Source}");
					sb.AppendLine($"StackTrace: {ex.StackTrace}");
					if (ex.InnerException != null)
					{
						sb.AppendLine("----- Inner Exception -----");
						sb.AppendLine($"Message: {ex.InnerException.Message}");
						sb.AppendLine($"Source: {ex.InnerException.Source}");
						sb.AppendLine($"StackTrace: {ex.InnerException.StackTrace}");
					}
					sb.AppendLine("============================");
					sb.AppendLine();
				}
				string tempFilePath = Path.GetTempFileName();

				using (FileStream tempFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
				using (StreamWriter writer = new StreamWriter(tempFileStream))
				{
					writer.Write(sb.ToString());
					writer.Flush();

					if (File.Exists(_logFilePath))
					{
						using (FileStream logFileStream = new FileStream(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
						{
							logFileStream.CopyTo(tempFileStream);
						}
					}
				}
				File.Copy(tempFilePath, _logFilePath, true);
				File.Delete(tempFilePath);
			}
			catch(Exception loggingEx)
			{
				Console.WriteLine($"An error occurred while logging the exception: {loggingEx.Message}");
			}
		}

		private void CheckAndRotateLogFile()
		{
			try
			{
				if (File.Exists(_logFilePath))
				{
					FileInfo logFileInfo = new FileInfo(_logFilePath);
					if( (logFileInfo.Length > _maxLogFileSizeInBytes))
					{
						string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
						string backupLogFilePath = $"ExceptionLog_{timestamp}.txt";

						File.Move(_logFilePath, backupLogFilePath);
						Console.WriteLine($"Log File rotated. Old log saved as:{backupLogFilePath}");
					}
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine("An error occurred while rotating the log file: " + ex.Message);
			}
		}
	}
}
