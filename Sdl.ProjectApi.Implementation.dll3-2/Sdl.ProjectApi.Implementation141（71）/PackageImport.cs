using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public abstract class PackageImport : PackageOperation, IPackageImport, IPackageOperation
	{
		internal Sdl.ProjectApi.Implementation.Xml.PackageImport XmlPackageImport => (Sdl.ProjectApi.Implementation.Xml.PackageImport)base.XmlPackageOperation;

		protected IProjectPathUtil ProjectPathUtil { get; }

		internal PackageImport(Sdl.ProjectApi.Implementation.Xml.PackageImport xmlPackageImport, IProjectPathUtil projectPathUtil)
			: base(xmlPackageImport)
		{
			ProjectPathUtil = projectPathUtil;
		}

		internal PackageImport(IProject project, Sdl.ProjectApi.Implementation.Xml.PackageImport xmlPackageImport, IProjectPathUtil projectPathUtil)
			: base(project, xmlPackageImport)
		{
			ProjectPathUtil = projectPathUtil;
		}

		internal override string GetAbsolutePackagePath()
		{
			return ProjectPathUtil.MakeAbsolutePath(base.Project, XmlPackageImport.Path, false);
		}
	}
}
