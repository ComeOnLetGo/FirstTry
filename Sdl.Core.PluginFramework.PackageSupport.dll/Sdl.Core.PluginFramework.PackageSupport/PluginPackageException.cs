using System;
using System.Runtime.Serialization;

namespace Sdl.Core.PluginFramework.PackageSupport
{
	[Serializable]
	public class PluginPackageException : Exception
	{
		public PluginPackageException()
		{
		}

		public PluginPackageException(string message)
			: base(message)
		{
		}

		public PluginPackageException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected PluginPackageException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
