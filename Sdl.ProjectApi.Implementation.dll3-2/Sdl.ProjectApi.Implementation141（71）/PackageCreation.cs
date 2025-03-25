using System.IO;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public abstract class PackageCreation : PackageOperation, IPackageCreation, IPackageOperation
	{
		private const string TempPackageFolderRoot = "TmpPkg";

		private static readonly object _tempPackageFolderSyncObject = new object();

		internal Sdl.ProjectApi.Implementation.Xml.PackageCreation XmlPackageCreation => (Sdl.ProjectApi.Implementation.Xml.PackageCreation)base.XmlPackageOperation;

		protected IProjectPathUtil ProjectPathUtil { get; }

		internal PackageCreation(IProject project, Sdl.ProjectApi.Implementation.Xml.PackageOperation xmlPackageOperation, IProjectPathUtil projectPathUtil)
			: base(project, xmlPackageOperation)
		{
			ProjectPathUtil = projectPathUtil;
		}

		internal override string GetAbsolutePackagePath()
		{
			return ProjectPathUtil.MakeAbsolutePath(base.Project, XmlPackageCreation.Path, false);
		}

		public void DownloadPackage(string destinationFilePath)
		{
			Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath));
			Util.CopyFile(GetAbsolutePackagePath(), destinationFilePath);
		}

		protected string CreateTemporaryPackageFolder()
		{
			string text = string.Empty;
			lock (_tempPackageFolderSyncObject)
			{
				bool flag = false;
				int num = 1;
				while (!flag && num < 10000)
				{
					text = Path.Combine(Path.GetTempPath(), "TmpPkg" + num);
					text += "\\";
					if (!Directory.Exists(text))
					{
						flag = true;
					}
					num++;
				}
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
			}
			return text;
		}
	}
}
