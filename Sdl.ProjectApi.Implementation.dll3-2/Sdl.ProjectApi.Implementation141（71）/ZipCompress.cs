using System;
using System.IO;
using System.Text;
using SharpCompress.Common;
using SharpCompress.Common.Zip;
using SharpCompress.Readers;
using SharpCompress.Readers.Zip;
using SharpCompress.Writers;

namespace Sdl.ProjectApi.Implementation
{
	public class ZipCompress : IZipCompress
	{
		public void ZipDirectory(string packageFilepath, string localDataFolder)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_0029: Expected O, but got Unknown
			using Stream stream = File.OpenWrite(packageFilepath);
			IWriter val = WriterFactory.Open(stream, (ArchiveType)1, new WriterOptions((CompressionType)4)
			{
				ArchiveEncoding = new ArchiveEncoding(Encoding.UTF8, Encoding.UTF8)
			});
			try
			{
				IWriterExtensions.WriteAll(val, localDataFolder, "*", SearchOption.AllDirectories);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}

		public void UnZip(string zipfile, string targetDir)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Expected O, but got Unknown
			//IL_0022: Expected O, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected O, but got Unknown
			using Stream stream = File.OpenRead(zipfile);
			ReaderOptions val = new ReaderOptions
			{
				ArchiveEncoding = new ArchiveEncoding(Encoding.UTF8, Encoding.UTF8)
			};
			ZipReader val2 = ZipReader.Open(stream, val);
			try
			{
				if (!((AbstractReader<ZipEntry, ZipVolume>)(object)val2).MoveToNextEntry() && ((AbstractReader<ZipEntry, ZipVolume>)(object)val2).Entry == null)
				{
					throw new InvalidOperationException();
				}
				do
				{
					ZipEntry entry = ((AbstractReader<ZipEntry, ZipVolume>)(object)val2).Entry;
					if (!((Entry)entry).IsDirectory)
					{
						IReaderExtensions.WriteEntryToDirectory((IReader)(object)val2, targetDir, new ExtractionOptions
						{
							ExtractFullPath = true,
							PreserveFileTime = true
						});
					}
				}
				while (((AbstractReader<ZipEntry, ZipVolume>)(object)val2).MoveToNextEntry());
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
	}
}
