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
	public class FileVersion : GenericItem
	{
		private int versionNumberField;

		private long sizeField;

		private string fileNameField;

		private string physicalPathField;

		private string commentField;

		private string createdByField;

		private DateTime createdAtField;

		private DateTime fileTimeStampField;

		private bool isAutoUploadField;

		[XmlAttribute]
		public int VersionNumber
		{
			get
			{
				return versionNumberField;
			}
			set
			{
				versionNumberField = value;
			}
		}

		[XmlAttribute]
		public long Size
		{
			get
			{
				return sizeField;
			}
			set
			{
				sizeField = value;
			}
		}

		[XmlAttribute]
		public string FileName
		{
			get
			{
				return fileNameField;
			}
			set
			{
				fileNameField = value;
			}
		}

		[XmlAttribute]
		public string PhysicalPath
		{
			get
			{
				return physicalPathField;
			}
			set
			{
				physicalPathField = value;
			}
		}

		[XmlAttribute]
		public string Comment
		{
			get
			{
				return commentField;
			}
			set
			{
				commentField = value;
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
		public DateTime FileTimeStamp
		{
			get
			{
				return fileTimeStampField;
			}
			set
			{
				fileTimeStampField = value;
			}
		}

		[XmlAttribute]
		public bool IsAutoUpload
		{
			get
			{
				return isAutoUploadField;
			}
			set
			{
				isAutoUploadField = value;
			}
		}
	}
}
