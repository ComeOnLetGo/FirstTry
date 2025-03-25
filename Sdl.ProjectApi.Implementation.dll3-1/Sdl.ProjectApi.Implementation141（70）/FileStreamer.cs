using System.IO;

namespace Sdl.ProjectApi.Implementation
{
	public class FileStreamer
	{
		private const int BLOCKSIZE_FILETRANSFER = 8192;

		public long BytesWritten { get; private set; }

		public void WriteFile(Stream inputStream, FileStream writeStream)
		{
			try
			{
				BytesWritten = 0L;
				byte[] array = new byte[8192];
				int num = 0;
				do
				{
					num = inputStream.Read(array, 0, array.Length);
					if (num > 0)
					{
						writeStream.Write(array, 0, num);
						BytesWritten += num;
					}
				}
				while (num > 0);
			}
			finally
			{
				writeStream?.Close();
			}
		}
	}
}
