using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Sdl.Core.PluginFramework.PackageSupport
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	public class PackageSupportResFile
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					resourceMan = new ResourceManager("Sdl.Core.PluginFramework.PackageSupport.PackageSupportResFile", typeof(PackageSupportResFile).Assembly);
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		public static string AttributeVersion_Missing_ErrorMessage => ResourceManager.GetString("AttributeVersion_Missing_ErrorMessage", resourceCulture);

		public static string MinOrMaxVersion_Incorrect_ErrorMessage => ResourceManager.GetString("MinOrMaxVersion_Incorrect_ErrorMessage", resourceCulture);

		public static string PluginName_Missing_ErrorMessage => ResourceManager.GetString("PluginName_Missing_ErrorMessage", resourceCulture);

		public static string PluginVersion_Incorrect_ErrorMessage => ResourceManager.GetString("PluginVersion_Incorrect_ErrorMessage", resourceCulture);

		public static string RequiredProduct_Missing_ErrorMessage => ResourceManager.GetString("RequiredProduct_Missing_ErrorMessage", resourceCulture);

		public static string RequiredProductValue_Missing_ErrorMessage => ResourceManager.GetString("RequiredProductValue_Missing_ErrorMessage", resourceCulture);

		public static string Version_Missing_ErrorMessage => ResourceManager.GetString("Version_Missing_ErrorMessage", resourceCulture);

		internal PackageSupportResFile()
		{
		}
	}
}
