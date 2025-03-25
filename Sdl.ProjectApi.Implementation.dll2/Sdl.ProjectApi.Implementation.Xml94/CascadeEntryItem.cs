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
	public class CascadeEntryItem
	{
		private TranslationProviderItem mainTranslationProviderItemField;

		private List<TranslationProviderItem> projectTranslationProviderItemField = new List<TranslationProviderItem>();

		private bool performUpdateField;

		private bool performNormalSearchField;

		private bool performConcordanceSearchField;

		private int penaltyField;

		[XmlElement(Order = 0)]
		public TranslationProviderItem MainTranslationProviderItem
		{
			get
			{
				return mainTranslationProviderItemField;
			}
			set
			{
				mainTranslationProviderItemField = value;
			}
		}

		[XmlElement("ProjectTranslationProviderItem", Order = 1)]
		public List<TranslationProviderItem> ProjectTranslationProviderItem
		{
			get
			{
				return projectTranslationProviderItemField;
			}
			set
			{
				projectTranslationProviderItemField = value;
			}
		}

		[XmlAttribute]
		public bool PerformUpdate
		{
			get
			{
				return performUpdateField;
			}
			set
			{
				performUpdateField = value;
			}
		}

		[XmlAttribute]
		public bool PerformNormalSearch
		{
			get
			{
				return performNormalSearchField;
			}
			set
			{
				performNormalSearchField = value;
			}
		}

		[XmlAttribute]
		public bool PerformConcordanceSearch
		{
			get
			{
				return performConcordanceSearchField;
			}
			set
			{
				performConcordanceSearchField = value;
			}
		}

		[XmlAttribute]
		public int Penalty
		{
			get
			{
				return penaltyField;
			}
			set
			{
				penaltyField = value;
			}
		}

		public CascadeEntryItem Copy()
		{
			CascadeEntryItem cascadeEntryItem = new CascadeEntryItem();
			cascadeEntryItem.MainTranslationProviderItem = MainTranslationProviderItem.Copy();
			cascadeEntryItem.Penalty = Penalty;
			cascadeEntryItem.PerformConcordanceSearch = PerformConcordanceSearch;
			cascadeEntryItem.PerformNormalSearch = PerformNormalSearch;
			cascadeEntryItem.PerformUpdate = PerformUpdate;
			cascadeEntryItem.ProjectTranslationProviderItem = new List<TranslationProviderItem>();
			if (ProjectTranslationProviderItem != null)
			{
				foreach (TranslationProviderItem item in ProjectTranslationProviderItem)
				{
					cascadeEntryItem.ProjectTranslationProviderItem.Add(item.Copy());
				}
			}
			return cascadeEntryItem;
		}
	}
}
