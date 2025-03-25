using System;
using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Interfaces
{
	internal interface IPackageProjectRepository : IProjectRepository, IProjectConfigurationRepository
	{
		PackageLicenseInfo PackageLicenseInfo { get; }

		Guid PackageGuid { get; set; }

		PackageType PackageType { get; }

		string Comment { get; set; }

		DateTime PackageCreatedAt { get; }

		string PackageCreatedBy { get; }

		void AddPackageTask(IManualTask task);

		List<IManualTask> GetPackageTasks(PackageProject packageProject);
	}
}
