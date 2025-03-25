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
	public class User
	{
		private string userIdField;

		private string fullNameField;

		private string descriptionField;

		private string emailField;

		private string phoneNumberField;

		private EmailType emailTypeField;

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

		[XmlAttribute]
		public string FullName
		{
			get
			{
				return fullNameField;
			}
			set
			{
				fullNameField = value;
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
		public string Email
		{
			get
			{
				return emailField;
			}
			set
			{
				emailField = value;
			}
		}

		[XmlAttribute]
		public string PhoneNumber
		{
			get
			{
				return phoneNumberField;
			}
			set
			{
				phoneNumberField = value;
			}
		}

		[XmlAttribute]
		public EmailType EmailType
		{
			get
			{
				return emailTypeField;
			}
			set
			{
				emailTypeField = value;
			}
		}
	}
}
