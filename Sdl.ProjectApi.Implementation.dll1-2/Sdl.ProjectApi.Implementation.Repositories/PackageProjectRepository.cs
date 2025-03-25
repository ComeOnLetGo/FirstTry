using System;
using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	internal class PackageProjectRepository : ProjectRepository, IPackageProjectRepository, IProjectRepository, IProjectConfigurationRepository
	{
		private Sdl.ProjectApi.Implementation.Xml.PackageProject _xmlPackageProject;

		public PackageLicenseInfo PackageLicenseInfo => _xmlPackageProject.PackageLicenseInfo;

		protected override Sdl.ProjectApi.Implementation.Xml.Project XmlProject => _xmlPackageProject;

		protected override ProjectConfiguration XmlConfiguration => _xmlPackageProject;

		public Guid PackageGuid
		{
			get
			{
				return _xmlPackageProject.PackageGuid;
			}
			set
			{
				_xmlPackageProject.PackageGuid = value;
			}
		}

		public PackageType PackageType => EnumConvert.ConvertPackageType(_xmlPackageProject.PackageType);

		public string Comment
		{
			get
			{
				return _xmlPackageProject.Comment;
			}
			set
			{
				_xmlPackageProject.Comment = value;
			}
		}

		public DateTime PackageCreatedAt => _xmlPackageProject.PackageCreatedAt;

		public string PackageCreatedBy => _xmlPackageProject.PackageCreatedBy;

		public PackageProjectRepository(IApplication application, IProjectPathUtil pathUtil)
			: base(application, pathUtil)
		{
			_xmlPackageProject = new Sdl.ProjectApi.Implementation.Xml.PackageProject();
		}

		public PackageProjectRepository(IApplication application, IProjectPathUtil pathUtil, string name, PackageType packageType, Guid originalProjectGuid, IUser packageCreatedBy, IUser projectCreatedBy, DateTime projectCreatedAt, Language sourceLanguage)
			: base(application, pathUtil)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			_xmlPackageProject = new Sdl.ProjectApi.Implementation.Xml.PackageProject();
			Initialize(name, Guid.NewGuid(), projectCreatedBy, projectCreatedAt, inPlace: false, sourceLanguage);
			_xmlPackageProject.PackageType = EnumConvert.ConvertPackageType(packageType);
			_xmlPackageProject.PackageGuid = Guid.NewGuid();
			_xmlPackageProject.PackageCreatedAt = DateTime.UtcNow;
			_xmlPackageProject.PackageCreatedBy = packageCreatedBy.UserId;
			_xmlPackageProject.Guid = originalProjectGuid;
			MarkAsInitialized();
		}

		public void AddPackageTask(IManualTask task)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			TaskRef taskRef = new TaskRef();
			TaskId id = ((ITaskBase)task).Id;
			taskRef.TaskGuid = ((TaskId)(ref id)).ToGuidArray()[0];
			_xmlPackageProject.PackageTasks.Add(taskRef);
		}

		public List<IManualTask> GetPackageTasks(PackageProject packageProject)
		{
			List<IManualTask> list = new List<IManualTask>();
			foreach (TaskRef packageTask in _xmlPackageProject.PackageTasks)
			{
				ManualTask manualTask = packageProject.GetManualTask(packageTask.TaskGuid);
				if (manualTask == null)
				{
					throw new InvalidProjectDataException($"Could not find package task with id {packageTask.TaskGuid}");
				}
				list.Add((IManualTask)(object)manualTask);
			}
			return list;
		}

		protected override Sdl.ProjectApi.Implementation.Xml.Project Deserialize(string projectFilePath)
		{
			_xmlPackageProject = Sdl.ProjectApi.Implementation.Xml.PackageProject.Deserialize(projectFilePath);
			return _xmlPackageProject;
		}

		public override void Save(string projectFilePath)
		{
			base.SettingsBundles.Save();
			_xmlPackageProject.Serialize(projectFilePath);
		}
	}
}
