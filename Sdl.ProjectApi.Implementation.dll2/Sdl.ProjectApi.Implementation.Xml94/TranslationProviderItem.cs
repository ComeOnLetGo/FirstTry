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
	public class TranslationProviderItem
	{
		private string uriField;

		private string stateField;

		private bool enabledField;

		[XmlAttribute]
		public string Uri
		{
			get
			{
				return uriField;
			}
			set
			{
				uriField = value;
			}
		}

		[XmlAttribute]
		public string State
		{
			get
			{
				return stateField;
			}
			set
			{
				stateField = value;
			}
		}

		[XmlAttribute]
		public bool Enabled
		{
			get
			{
				return enabledField;
			}
			set
			{
				enabledField = value;
			}
		}

		public TranslationProviderItem Copy()
		{
			TranslationProviderItem translationProviderItem = new TranslationProviderItem();
			translationProviderItem.Uri = Uri;
			translationProviderItem.State = State;
			translationProviderItem.Enabled = Enabled;
			return translationProviderItem;
		}
	}
}
