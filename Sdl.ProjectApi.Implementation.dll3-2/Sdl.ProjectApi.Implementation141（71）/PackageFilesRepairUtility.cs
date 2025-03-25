using System.Collections.Generic;
using System.IO;

namespace Sdl.ProjectApi.Implementation
{
	public class PackageFilesRepairUtility
	{
		private readonly IProject _package;

		public PackageFilesRepairUtility(IProject package)
		{
			_package = package;
		}

		public bool CheckIfRepairRequired()
		{
			if (!AreProjectFilesMissing())
			{
				return AreReportFilesMissing();
			}
			return true;
		}

		public bool RepairDamagedFiles()
		{
			bool flag = RepairDamagedProjectFiles();
			return RepairDamagedReportFiles();
		}

		private bool AreProjectFilesMissing()
		{
			IProjectFile[] allProjectFiles = _package.GetAllProjectFiles();
			IProjectFile[] array = allProjectFiles;
			foreach (IProjectFile val in array)
			{
				if (!File.Exists(val.LocalFilePath))
				{
					return true;
				}
			}
			return false;
		}

		private bool AreReportFilesMissing()
		{
			IReport[] array = CollectReports();
			DirectoryInfo parent = new DirectoryInfo(_package.ProjectFilePath).Parent;
			IReport[] array2 = array;
			foreach (IReport val in array2)
			{
				string path = parent.FullName + "\\" + val.PhysicalPath;
				if (!File.Exists(path))
				{
					return true;
				}
			}
			return false;
		}

		private IReport[] CollectReports()
		{
			List<IReport> list = new List<IReport>();
			IAutomaticTask[] automaticTasks = _package.AutomaticTasks;
			foreach (IAutomaticTask val in automaticTasks)
			{
				list.AddRange(((ITaskBase)val).Reports);
			}
			return list.ToArray();
		}

		private bool RepairDamagedReportFiles()
		{
			IReport[] array = CollectReports();
			DirectoryInfo parent = new DirectoryInfo(_package.ProjectFilePath).Parent;
			IReport[] array2 = array;
			foreach (IReport val in array2)
			{
				string text = parent.FullName + "\\" + val.PhysicalPath;
				if (!File.Exists(text))
				{
					string mostSimilarFileFromDirectory = GetMostSimilarFileFromDirectory(Path.GetFileName(text), new DirectoryInfo(Path.GetDirectoryName(text)));
					if (string.IsNullOrEmpty(mostSimilarFileFromDirectory))
					{
						return false;
					}
					File.Move(mostSimilarFileFromDirectory, text);
				}
			}
			return true;
		}

		private bool RepairDamagedProjectFiles()
		{
			IProjectFile[] allProjectFiles = _package.GetAllProjectFiles();
			IProjectFile[] array = allProjectFiles;
			foreach (IProjectFile val in array)
			{
				if (!File.Exists(val.LocalFilePath))
				{
					List<string> mathcesBasedOnSize = GetMathcesBasedOnSize(val);
					if (mathcesBasedOnSize.Count == 0)
					{
						return false;
					}
					if (mathcesBasedOnSize.Count == 1)
					{
						File.Move(mathcesBasedOnSize[0], val.LocalFilePath);
						continue;
					}
					DirectoryInfo parent = new DirectoryInfo(val.LocalFilePath).Parent;
					string mostSimilarFileFromDirectory = GetMostSimilarFileFromDirectory(val.Filename, parent);
					File.Move(mostSimilarFileFromDirectory, val.LocalFilePath);
				}
			}
			return true;
		}

		private List<string> GetMathcesBasedOnSize(IProjectFile projectFile)
		{
			List<string> list = new List<string>();
			DirectoryInfo parent = new DirectoryInfo(projectFile.LocalFilePath).Parent;
			FileInfo[] files = parent.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				if (fileInfo.Length == projectFile.Size)
				{
					list.Add(fileInfo.FullName);
				}
			}
			return list;
		}

		private string GetMostSimilarFileFromDirectory(string fileName, DirectoryInfo directory)
		{
			string result = string.Empty;
			int num = int.MaxValue;
			FileInfo[] files = directory.GetFiles();
			foreach (FileInfo fileInfo in files)
			{
				int num2 = LevenshteinDistance.Compute(fileName, fileInfo.Name);
				if (num2 < num)
				{
					num = num2;
					result = fileInfo.FullName;
				}
			}
			return result;
		}
	}
}
