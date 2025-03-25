using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class ProjectFile : GenericItem
	{
		private List<string> specificTargetLanguagesField = new List<string>();

		private List<LanguageFile> languageFilesField = new List<LanguageFile>();

		private List<PreviousBilingualFile> previousBilingualFilesField = new List<PreviousBilingualFile>();

		private Guid parentProjectFileGuidField = Guid.Empty;

		private string filterDefinitionIdField;

		private string nameField;

		private string pathField;

		private FileRole roleField;

		[XmlArray(Order = 0)]
		[XmlArrayItem("SpecificTargetLanguage", IsNullable = false)]
		public List<string> SpecificTargetLanguages
		{
			get
			{
				return specificTargetLanguagesField;
			}
			set
			{
				specificTargetLanguagesField = value;
			}
		}

		[XmlArray(Order = 1)]
		[XmlArrayItem(IsNullable = false)]
		public List<LanguageFile> LanguageFiles
		{
			get
			{
				return languageFilesField;
			}
			set
			{
				languageFilesField = value;
			}
		}

		[XmlArray(Order = 2)]
		[XmlArrayItem(IsNullable = false)]
		public List<PreviousBilingualFile> PreviousBilingualFiles
		{
			get
			{
				return previousBilingualFilesField;
			}
			set
			{
				previousBilingualFilesField = value;
			}
		}

		[XmlAttribute]
		public Guid ParentProjectFileGuid
		{
			get
			{
				return parentProjectFileGuidField;
			}
			set
			{
				parentProjectFileGuidField = value;
			}
		}

		[XmlAttribute]
		public string FilterDefinitionId
		{
			get
			{
				return filterDefinitionIdField;
			}
			set
			{
				filterDefinitionIdField = value;
			}
		}

		[XmlAttribute]
		public string Name
		{
			get
			{
				return nameField;
			}
			set
			{
				nameField = value;
			}
		}

		[XmlAttribute]
		public string Path
		{
			get
			{
				return pathField;
			}
			set
			{
				pathField = value;
			}
		}

		[XmlAttribute]
		public FileRole Role
		{
			get
			{
				return roleField;
			}
			set
			{
				roleField = value;
			}
		}

		public LanguageFile GetLanguageFileByLanguage(string languageCode)
		{
			return LanguageFiles.Find(new LanguageCodePredicate(languageCode).MatchLanguage);
		}
	}
}
