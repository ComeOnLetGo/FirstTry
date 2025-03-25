using System.IO;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class ReportRepository : IReportRepository
	{
		private readonly string _absoluteReportFileName;

		public ReportRepository(string absoluteReportFileName)
		{
			_absoluteReportFileName = absoluteReportFileName;
		}

		public void SetReportXml(string reportXml)
		{
			using StreamWriter streamWriter = File.CreateText(_absoluteReportFileName);
			streamWriter.Write(reportXml);
		}

		public void SetReportFile(string reportFile)
		{
			Util.CopyFile(reportFile, _absoluteReportFileName);
		}

		public string GetReportXml()
		{
			using Stream stream = GetReportDownloadStream();
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
		}

		public Stream GetReportDownloadStream()
		{
			return File.OpenRead(_absoluteReportFileName);
		}

		public void DownloadReport(string targetFilePath)
		{
			File.Copy(_absoluteReportFileName, targetFilePath);
		}

		public void SaveAs(string targetFilePath, byte[] report)
		{
			using Stream stream = File.Create(targetFilePath);
			stream.Write(report, 0, report.Length);
		}

		public byte[] GetCustomTemplate(string path)
		{
			return File.ReadAllBytes(path);
		}
	}
}
