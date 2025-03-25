using System.IO;

namespace Sdl.Core.PluginFramework.PackageSupport
{
	public class StreamUtils
	{
		public static void CopyStream(Stream source, Stream target)
		{
			byte[] buffer = new byte[4096];
			int num = 0;
			while ((num = source.Read(buffer, 0, 4096)) > 0)
			{
				target.Write(buffer, 0, num);
			}
		}

		public static bool CompareStream(Stream source, Stream target)
		{
			byte[] array = new byte[4096];
			byte[] array2 = new byte[4096];
			int num = source.Read(array, 0, 4096);
			int num2 = target.Read(array2, 0, 4096);
			while (num == num2 && num2 > 0)
			{
				for (int i = 0; i < num2; i++)
				{
					if (array[i] != array2[i])
					{
						return false;
					}
				}
				num = source.Read(array, 0, 4096);
				num2 = target.Read(array2, 0, 4096);
			}
			return num == num2;
		}
	}
}
