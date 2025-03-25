using System;
using System.IO;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Builders
{
	internal class XmlProjectPackageCreationBuilder
	{
		private readonly IProjectPathUtil _projectPathUtil;

		public XmlProjectPackageCreationBuilder(IProjectPathUtil projectPathUtil)
		{
			_projectPathUtil = projectPathUtil;
		}

		public Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation CreateXmlPackageCreation(IProject project)
		{
			string packageName = "PublishProject";
			string absolutePath = CreatePackageFilePath(project, packageName, (PackageType)0, imported: false);
			Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation projectPackageCreation = CreateProjectPackageCreation(project, packageName, absolutePath);
			projectPackageCreation.PackageGuid = Guid.NewGuid();
			return projectPackageCreation;
		}

		private Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation CreateProjectPackageCreation(IProject project, string packageName, string absolutePath)
		{
			return new Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation
			{
				Guid = Guid.NewGuid(),
				Path = _projectPathUtil.MakeRelativePath(project, absolutePath, false),
				PercentComplete = 0,
				Status = PackageStatus.Scheduled,
				PackageName = packageName
			};
		}

		internal static string CreatePackageFilePath(IProject project, string packageName, PackageType packageType, bool imported)
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			DateTime now = DateTime.Now;
			string path = $"{packageName}-{now.Year}{now.Month}{now.Day}-{now.Hour}h{now.Minute}m{now.Second}s{(((int)packageType == 0) ? FileTypes.ProjectPackageFileExtension : FileTypes.ReturnPackageFileExtension)}";
			return Path.Combine(Path.Combine(project.LocalDataFolder, imported ? "Packages\\In" : "Packages\\Out"), path);
		}

		public Sdl.ProjectApi.Implementation.Xml.ReturnPackageCreation CreateXmlReturnPackageCreation(IProject project, ITaskFile[] taskFiles, string packageName, string comment)
		{
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			string text = CreatePackageFilePath(project, packageName, (PackageType)1, imported: false);
			Sdl.ProjectApi.Implementation.Xml.ReturnPackageCreation returnPackageCreation = new Sdl.ProjectApi.Implementation.Xml.ReturnPackageCreation
			{
				Guid = Guid.NewGuid(),
				Path = ((IProjectConfiguration)project).ProjectsProvider.PathUtil.MakeRelativePath(project, text, false),
				PercentComplete = 0,
				Status = PackageStatus.Scheduled,
				PackageName = packageName,
				Comment = comment
			};
			foreach (ITaskFile val in taskFiles)
			{
				TaskId id = val.Task.Id;
				Guid taskId = ((TaskId)(ref id)).ToGuidArray()[0];
				if (!returnPackageCreation.Tasks.Exists((TaskRef tr) => tr.TaskGuid == taskId))
				{
					TaskRef item = new TaskRef
					{
						TaskGuid = taskId
					};
					returnPackageCreation.Tasks.Add(item);
				}
				TaskFileRef item2 = new TaskFileRef
				{
					TaskFileGuid = val.Id
				};
				returnPackageCreation.Files.Add(item2);
			}
			return returnPackageCreation;
		}

		public Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation CreateXmlProjectPackageCreation(IProject project, IManualTask[] tasks, string packageName, string comment)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			string absolutePath = CreatePackageFilePath(project, packageName, (PackageType)0, imported: false);
			Sdl.ProjectApi.Implementation.Xml.ProjectPackageCreation projectPackageCreation = CreateProjectPackageCreation(project, packageName, absolutePath);
			projectPackageCreation.Comment = comment;
			foreach (IManualTask val in tasks)
			{
				TaskRef taskRef = new TaskRef();
				TaskId id = ((ITaskBase)val).Id;
				taskRef.TaskGuid = ((TaskId)(ref id)).ToGuidArray()[0];
				projectPackageCreation.Tasks.Add(taskRef);
				IManualTaskFile[] translatableFiles = val.TranslatableFiles;
				for (int j = 0; j < translatableFiles.Length; j++)
				{
					ITaskFile val2 = (ITaskFile)(object)translatableFiles[j];
					TaskFileRef taskFileRef = new TaskFileRef();
					taskFileRef.TaskFileGuid = val2.Id;
					projectPackageCreation.Files.Add(taskFileRef);
				}
			}
			return projectPackageCreation;
		}

		public Sdl.ProjectApi.Implementation.Xml.ProjectPackageImport CreateXmlProjectPackageImport(string packagePath)
		{
			return new Sdl.ProjectApi.Implementation.Xml.ProjectPackageImport
			{
				Guid = Guid.NewGuid(),
				Path = packagePath,
				PercentComplete = 0,
				Status = PackageStatus.Scheduled,
				PackageName = Path.GetFileNameWithoutExtension(packagePath)
			};
		}

		public Sdl.ProjectApi.Implementation.Xml.ReturnPackageImport CreateXmlReturnPackageImport(string packagePath)
		{
			return new Sdl.ProjectApi.Implementation.Xml.ReturnPackageImport
			{
				Guid = Guid.NewGuid(),
				Path = packagePath,
				PercentComplete = 0,
				Status = PackageStatus.Scheduled,
				PackageName = Path.GetFileNameWithoutExtension(packagePath)
			};
		}
	}
}
