using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace Sdl.ProjectApi.Implementation.Xml
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[DesignerCategory("code")]
	public class GeneralProjectInfo
	{
		private Customer customerField;

		private string nameField;

		private string descriptionField;

		private bool isImportedField;

		private DateTime createdAtField;

		private string createdByField;

		private ProjectStatus statusField;

		private DateTime startedAtField;

		private bool startedAtFieldSpecified;

		private DateTime completedAtField;

		private bool completedAtFieldSpecified;

		private DateTime archivedAtField;

		private bool archivedAtFieldSpecified;

		private DateTime dueDateField;

		private bool dueDateFieldSpecified;

		private bool isInPlaceField;

		private string packageConvertorField;

		private bool isSecureField;

		private bool isSecureFieldSpecified;

		private bool isCloudBasedField;

		private string languageCloudLocationField;

		[XmlElement(Order = 0)]
		public Customer Customer
		{
			get
			{
				return customerField;
			}
			set
			{
				customerField = value;
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
		public string Description
		{
			get
			{
				return descriptionField;
			}
			set
			{
				descriptionField = value;
			}
		}

		[XmlAttribute]
		public bool IsImported
		{
			get
			{
				return isImportedField;
			}
			set
			{
				isImportedField = value;
			}
		}

		[XmlAttribute]
		public DateTime CreatedAt
		{
			get
			{
				return createdAtField;
			}
			set
			{
				createdAtField = value;
			}
		}

		[XmlAttribute]
		public string CreatedBy
		{
			get
			{
				return createdByField;
			}
			set
			{
				createdByField = value;
			}
		}

		[XmlAttribute]
		public ProjectStatus Status
		{
			get
			{
				return statusField;
			}
			set
			{
				statusField = value;
			}
		}

		[XmlAttribute]
		public DateTime StartedAt
		{
			get
			{
				return startedAtField;
			}
			set
			{
				startedAtField = value;
			}
		}

		[XmlIgnore]
		public bool StartedAtSpecified
		{
			get
			{
				return startedAtFieldSpecified;
			}
			set
			{
				startedAtFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public DateTime CompletedAt
		{
			get
			{
				return completedAtField;
			}
			set
			{
				completedAtField = value;
			}
		}

		[XmlIgnore]
		public bool CompletedAtSpecified
		{
			get
			{
				return completedAtFieldSpecified;
			}
			set
			{
				completedAtFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public DateTime ArchivedAt
		{
			get
			{
				return archivedAtField;
			}
			set
			{
				archivedAtField = value;
			}
		}

		[XmlIgnore]
		public bool ArchivedAtSpecified
		{
			get
			{
				return archivedAtFieldSpecified;
			}
			set
			{
				archivedAtFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public DateTime DueDate
		{
			get
			{
				return dueDateField;
			}
			set
			{
				dueDateField = value;
			}
		}

		[XmlIgnore]
		public bool DueDateSpecified
		{
			get
			{
				return dueDateFieldSpecified;
			}
			set
			{
				dueDateFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public bool IsInPlace
		{
			get
			{
				return isInPlaceField;
			}
			set
			{
				isInPlaceField = value;
			}
		}

		[XmlAttribute]
		public string PackageConvertor
		{
			get
			{
				return packageConvertorField;
			}
			set
			{
				packageConvertorField = value;
			}
		}

		[XmlAttribute]
		public bool IsSecure
		{
			get
			{
				return isSecureField;
			}
			set
			{
				isSecureField = value;
			}
		}

		[XmlIgnore]
		public bool IsSecureSpecified
		{
			get
			{
				return isSecureFieldSpecified;
			}
			set
			{
				isSecureFieldSpecified = value;
			}
		}

		[XmlAttribute]
		[DefaultValue(false)]
		public bool IsCloudBased
		{
			get
			{
				return isCloudBasedField;
			}
			set
			{
				isCloudBasedField = value;
			}
		}

		[XmlAttribute]
		public string LanguageCloudLocation
		{
			get
			{
				return languageCloudLocationField;
			}
			set
			{
				languageCloudLocationField = value;
			}
		}

		public GeneralProjectInfo()
		{
			isCloudBasedField = false;
		}
	}
}
