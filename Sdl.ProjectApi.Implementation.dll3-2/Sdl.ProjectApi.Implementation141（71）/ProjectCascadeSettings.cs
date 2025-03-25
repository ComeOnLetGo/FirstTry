using System;
using System.Collections.Generic;
using Sdl.LanguagePlatform.Core;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectCascadeSettings
	{
		private LanguagePair _languageDirection;

		public ProjectCascadeItem GeneralProjectCascadeItem { get; set; }

		public ProjectCascadeItem SpecificProjectCascadeItem { get; set; }

		public LanguagePair LanguageDirection
		{
			get
			{
				return _languageDirection;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value", "LanguageDirection");
				}
				_languageDirection = value;
			}
		}

		public bool RemoveDuplicates { get; set; }

		public ProjectCascadeEntryDataFilterFunction Filter { get; set; }

		public IComparer<ProjectCascadeEntryData> Sort { get; set; }

		public bool ReadOnly { get; set; }

		public ProjectCascadeSettings(ProjectCascadeItem generalProjectCascadeItem, ProjectCascadeItem specificProjectCascadeItem, LanguagePair languageDirection)
		{
			GeneralProjectCascadeItem = generalProjectCascadeItem;
			SpecificProjectCascadeItem = specificProjectCascadeItem;
			LanguageDirection = languageDirection;
			RemoveDuplicates = true;
			Filter = null;
			Sort = null;
			ReadOnly = true;
		}
	}
}
