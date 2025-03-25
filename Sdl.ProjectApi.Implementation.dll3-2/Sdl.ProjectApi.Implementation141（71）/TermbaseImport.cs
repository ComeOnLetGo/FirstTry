using System;
using System.Xml;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.MultiTerm.TMO.Interop;

namespace Sdl.ProjectApi.Implementation
{
	public class TermbaseImport
	{
		private readonly IApplication _multiTermApi;

		private readonly IFile _fileWrapper;

		public TermbaseImport(IApplication multiTermApi, IFile fileWrapper)
		{
			_multiTermApi = multiTermApi;
			_fileWrapper = fileWrapper;
		}

		public string Import(string termbaseName, string xmlFile)
		{
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				string text = xmlFile.Replace(".xml", ".xdt");
				if (!_fileWrapper.Exists(text))
				{
					throw new ProjectApiException(string.Format(ErrorMessages.ProjectPackageImport_FileNotFound, text, termbaseName));
				}
				string text2 = xmlFile.Replace(".xml", ".sdltb");
				if (_fileWrapper.Exists(text2))
				{
					_fileWrapper.Delete(text2);
				}
				((ITermbaseRepository)_multiTermApi.LocalRepository).Connect(string.Empty, string.Empty);
				Termbase val = ((ITermbases)((ITermbaseRepository)_multiTermApi.LocalRepository).Termbases).New(termbaseName, StringResources.ProjectPackageImport_LCTermbaseDescription, text, text2);
				((IImportDefinitions)((ITermbase)val).ImportDefinitions).Refresh();
				if (((IImportDefinitions)((ITermbase)val).ImportDefinitions).Count > 0)
				{
					ProcessImport(val, ((IImportDefinition)((IImportDefinitions)((ITermbase)val).ImportDefinitions)[(object)0]).Name, xmlFile);
				}
				((ITermbase)val).Close();
				return text2;
			}
			catch (Exception)
			{
				throw new ProjectApiException(string.Format(ErrorMessages.ProjectPackageImport_Error, termbaseName));
			}
			finally
			{
				IApplication multiTermApi = _multiTermApi;
				if (multiTermApi != null)
				{
					((ITermbaseRepository)multiTermApi.LocalRepository).Disconnect();
				}
			}
		}

		private void ProcessImport(Termbase termbase, string templateName, string importFile)
		{
			string text = Environment.GetEnvironmentVariable("TEMP") + "\\tmpImportDefinition.xml";
			((IImportDefinition)((IImportDefinitions)((ITermbase)termbase).ImportDefinitions)[(object)templateName]).Save(text);
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load(text);
			XmlNode xmlNode = xmlDocument.SelectSingleNode("//XMLFile");
			xmlNode.InnerXml = importFile;
			xmlNode = xmlDocument.SelectSingleNode("//ErrorFile");
			xmlNode.InnerXml = importFile + ".import.log";
			xmlDocument.Save(text);
			ImportDefinition val = ((IImportDefinitions)((ITermbase)termbase).ImportDefinitions).Add("tmpImportTemplate", "", text);
			((IImportDefinition)val).ProcessImport((MtTaskType)2, "", "");
			((IImportDefinition)val).Delete();
			_fileWrapper.Delete(text);
		}
	}
}
