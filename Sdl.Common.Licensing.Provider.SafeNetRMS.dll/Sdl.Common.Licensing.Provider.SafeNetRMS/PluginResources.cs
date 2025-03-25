using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Sdl.Common.Licensing.Provider.SafeNetRMS
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class PluginResources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					ResourceManager resourceManager = new ResourceManager("Sdl.Common.Licensing.Provider.SafeNetRMS.PluginResources", typeof(PluginResources).Assembly);
					resourceMan = resourceManager;
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
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

		internal static string Plugin_Description => ResourceManager.GetString("Plugin_Description", resourceCulture);

		internal static string Plugin_Name => ResourceManager.GetString("Plugin_Name", resourceCulture);

		internal PluginResources()
		{
		}
	}
}
