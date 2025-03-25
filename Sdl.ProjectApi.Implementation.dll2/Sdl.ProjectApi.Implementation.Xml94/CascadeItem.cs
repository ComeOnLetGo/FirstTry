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
	public class CascadeItem
	{
		private List<CascadeEntryItem> cascadeEntryItemField = new List<CascadeEntryItem>();

		private bool stopSearchingWhenResultsFoundField;

		private bool overrideParentField;

		[XmlElement("CascadeEntryItem", Order = 0)]
		public List<CascadeEntryItem> CascadeEntryItem
		{
			get
			{
				return cascadeEntryItemField;
			}
			set
			{
				cascadeEntryItemField = value;
			}
		}

		[XmlAttribute]
		public bool StopSearchingWhenResultsFound
		{
			get
			{
				return stopSearchingWhenResultsFoundField;
			}
			set
			{
				stopSearchingWhenResultsFoundField = value;
			}
		}

		[XmlAttribute]
		public bool OverrideParent
		{
			get
			{
				return overrideParentField;
			}
			set
			{
				overrideParentField = value;
			}
		}

		public CascadeItem Copy()
		{
			CascadeItem cascadeItem = new CascadeItem();
			cascadeItem.StopSearchingWhenResultsFound = StopSearchingWhenResultsFound;
			cascadeItem.OverrideParent = OverrideParent;
			cascadeItem.CascadeEntryItem = new List<CascadeEntryItem>();
			if (CascadeEntryItem != null)
			{
				foreach (CascadeEntryItem item in CascadeEntryItem)
				{
					cascadeItem.CascadeEntryItem.Add(item.Copy());
				}
			}
			return cascadeItem;
		}
	}
}
