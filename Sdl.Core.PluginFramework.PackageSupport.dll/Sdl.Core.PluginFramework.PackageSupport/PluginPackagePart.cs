using System;
using System.IO;
using System.IO.Packaging;

namespace Sdl.Core.PluginFramework.PackageSupport
{
	public class PluginPackagePart
	{
		public string Name => Path.GetFileName(RelativePath);

		public string Directory => Path.GetDirectoryName(RelativePath);

		private string RelativePath => Uri.UnescapeDataString(Part.Uri.ToString()).TrimStart('/').Replace('/', '\\');

		internal PackageRelationship Relationship { get; set; }

		internal PackagePart Part { get; set; }

		internal PluginPackagePart(PackageRelationship relationship, PackagePart part)
		{
			Relationship = relationship;
			Part = part;
		}
	}
}
