using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Text;

namespace Sdl.ProjectApi.Implementation
{
	internal class Util
	{
		public sealed class DeserializationBinder : SerializationBinder
		{
			public override Type BindToType(string assemblyName, string typeName)
			{
				if (!typeName.EndsWith("Exception"))
				{
					throw new SerializationException("Only Exception type is allowed");
				}
				return Type.GetType($"{typeName}, {assemblyName}");
			}
		}

		private static readonly Random rnd = new Random(DateTime.Now.Millisecond);

		private static readonly object syncObject = new object();

		public static void CopyFile(string sourceFilePath, string targetFilePath)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
			if (!object.Equals(sourceFilePath, targetFilePath))
			{
				File.Copy(sourceFilePath, targetFilePath, overwrite: true);
				File.SetAttributes(targetFilePath, FileAttributes.Normal);
				DateTime lastWriteTimeUtc = File.GetLastWriteTimeUtc(sourceFilePath);
				File.SetLastWriteTimeUtc(targetFilePath, lastWriteTimeUtc);
			}
		}

		public static void MoveFile(string sourceFilePath, string targetFilePath)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
			if (!object.Equals(sourceFilePath, targetFilePath))
			{
				if (File.Exists(targetFilePath))
				{
					File.Delete(targetFilePath);
				}
				File.Move(sourceFilePath, targetFilePath);
			}
		}

		public static long WriteStream(Stream inputStream, string targetFilePath, DateTime lastWriteTimeUtc)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath));
			FileStreamer fileStreamer = new FileStreamer();
			using (FileStream writeStream = File.Create(targetFilePath))
			{
				fileStreamer.WriteFile(inputStream, writeStream);
			}
			File.SetLastWriteTimeUtc(targetFilePath, lastWriteTimeUtc);
			return fileStreamer.BytesWritten;
		}

		public static Stream GetEmbeddedResourceStream(string resourceName)
		{
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
		}

		public static bool IsEmptyOrNonExistingDirectory(string path)
		{
			if (Directory.Exists(path))
			{
				if (Directory.GetFiles(path).Length == 0)
				{
					return Directory.GetDirectories(path).Length == 0;
				}
				return false;
			}
			return true;
		}

		public static void EnsureFilePathDirectoryExists(string path)
		{
			try
			{
				string directoryName = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
			}
			catch
			{
			}
		}

		public static string GetTaskExecutionPath(AutomaticTask automaticTask)
		{
			return GetTempFolder();
		}

		private static string GetTempFolder()
		{
			string tempFolderName = GetTempFolderName();
			while (Directory.Exists(tempFolderName))
			{
				tempFolderName = GetTempFolderName();
			}
			return tempFolderName;
		}

		private static string GetTempFolderName()
		{
			string text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			if (!string.IsNullOrEmpty(text))
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
				if (!string.IsNullOrEmpty(fileNameWithoutExtension))
				{
					return Path.Combine(Path.GetTempPath(), fileNameWithoutExtension);
				}
			}
			return string.Empty;
		}

		public static string GetTempFile(string extension)
		{
			string text = "temp";
			lock (syncObject)
			{
				string text2;
				while (File.Exists(text2 = Path.Combine(Path.GetTempPath(), text + rnd.Next() + "." + extension)))
				{
				}
				FileStream fileStream = File.Create(text2);
				fileStream.Close();
				return text2;
			}
		}

		public static void CheckArgumentNotNull(string parameterName, object parameterValue)
		{
			if (parameterValue == null)
			{
				throw new ArgumentNullException(parameterName);
			}
		}

		public static string GetXmlSerializationExceptionMessage(Exception ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			Exception ex2 = ex;
			do
			{
				stringBuilder.Append("\r\n");
				stringBuilder.Append(ex2.Message);
				if (!ex2.Message.EndsWith("."))
				{
					stringBuilder.Append('.');
				}
			}
			while ((ex2 = ex2.InnerException) != null);
			return stringBuilder.ToString();
		}

		public static string SerializeException(Exception ex)
		{
			using MemoryStream memoryStream = new MemoryStream();
			SoapFormatter soapFormatter = new SoapFormatter(null, new StreamingContext(StreamingContextStates.File));
			soapFormatter.Serialize(memoryStream, GetSerializableException(ex));
			return Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
		}

		private static Exception GetSerializableException(Exception ex)
		{
			bool flag = ex.GetType().IsSerializable;
			for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
			{
				if (!innerException.GetType().IsSerializable)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return ex;
			}
			return CreateSerializableException(ex);
		}

		private static Exception CreateSerializableException(Exception ex)
		{
			List<Exception> list = new List<Exception>();
			list.Add(ex);
			for (Exception innerException = ex.InnerException; innerException != null; innerException = innerException.InnerException)
			{
				list.Insert(0, innerException);
			}
			int i;
			for (i = -1; list[i + 1].GetType().IsSerializable; i++)
			{
			}
			if (i == list.Count - 1)
			{
				return ex;
			}
			Exception ex2 = ((i != -1) ? list[i] : null);
			for (int j = i + 1; j < list.Count; j++)
			{
				ex2 = CopyException(list[j], ex2);
			}
			return ex2;
		}

		private static Exception CopyException(Exception exception, Exception innerException)
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Expected O, but got Unknown
			if (!(exception is ProjectApiException))
			{
				return (Exception)new ProjectApiException($"Exception of type {exception.GetType().Name} occurred: {exception.Message}\r\nStacktrace:\r\n{exception.StackTrace}", innerException);
			}
			return (Exception)new ProjectApiException($"{exception.Message}\r\nStacktrace:\r\n{exception.StackTrace}", innerException);
		}

		public static Exception DeserializeException(string s)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			using MemoryStream serializationStream = new MemoryStream(bytes);
			SoapFormatter soapFormatter = new SoapFormatter(null, new StreamingContext(StreamingContextStates.File));
			soapFormatter.Binder = new DeserializationBinder();
			return (Exception)soapFormatter.Deserialize(serializationStream);
		}

		public static void CopyDirectory(DirectoryInfo diSource, DirectoryInfo diDestination, string FileFilter, string DirectoryFilter, bool Overwrite, int FolderLimit)
		{
			int i = 0;
			List<DirectoryInfo> list = new List<DirectoryInfo>();
			List<FileInfo> list2 = new List<FileInfo>();
			try
			{
				if (diSource == null)
				{
					throw new ArgumentException("Source Directory: NULL");
				}
				if (diDestination == null)
				{
					throw new ArgumentException("Destination Directory: NULL");
				}
				if (!diSource.Exists)
				{
					throw new IOException("Source Directory: Does Not Exist");
				}
				if (FolderLimit < 1 && FolderLimit != -1)
				{
					throw new ArgumentException("Folder Limit: Less Than 1");
				}
				if (DirectoryFilter == null || DirectoryFilter == string.Empty)
				{
					DirectoryFilter = "*";
				}
				if (FileFilter == null || FileFilter == string.Empty)
				{
					FileFilter = "*";
				}
				list.Add(diSource);
				for (; i < list.Count && (i < FolderLimit || FolderLimit == -1); i++)
				{
					DirectoryInfo[] directories = list[i].GetDirectories(DirectoryFilter);
					foreach (DirectoryInfo item in directories)
					{
						list.Add(item);
					}
					FileInfo[] files = list[i].GetFiles(FileFilter);
					foreach (FileInfo item2 in files)
					{
						list2.Add(item2);
					}
				}
				foreach (DirectoryInfo item3 in list)
				{
					if (item3.Exists)
					{
						string path = diDestination.FullName + Path.DirectorySeparatorChar + item3.FullName.Remove(0, diSource.FullName.Length);
						if (!Directory.Exists(path))
						{
							Directory.CreateDirectory(path);
						}
					}
				}
				foreach (FileInfo item4 in list2)
				{
					if (item4.Exists)
					{
						string text = diDestination.FullName + Path.DirectorySeparatorChar + item4.FullName.Remove(0, diSource.FullName.Length);
						if (Overwrite)
						{
							item4.CopyTo(text, overwrite: true);
						}
						else if (!File.Exists(text))
						{
							item4.CopyTo(text, overwrite: true);
						}
					}
				}
			}
			catch
			{
				throw;
			}
		}

		public static bool DoDirectoriesIntersect(string oldFolder, string newFolder)
		{
			int length = Math.Min(oldFolder.Length, newFolder.Length) + 1;
			return string.Compare(oldFolder + Path.DirectorySeparatorChar, 0, newFolder + Path.DirectorySeparatorChar, 0, length, ignoreCase: false) == 0;
		}
	}
}
