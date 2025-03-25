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
	public class LocalProjectServerInfo
	{
		private string localDataFolderField;

		private string userIdField;

		[XmlAttribute]
		public string LocalDataFolder
		{
			get
			{
				return localDataFolderField;
			}
			set
			{
				localDataFolderField = value;
			}
		}

		[XmlAttribute]
		public string UserId
		{
			get
			{
				return userIdField;
			}
			set
			{
				userIdField = value;
			}
		}
	}
}
