using System;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectCascadeEntryData : IProjectCascadeEntryData
	{
		public ProjectCascadeItem ProjectCascadeItem { get; private set; }

		public int ProjectCascadeEntryItemIndex { get; private set; }

		public ProjectCascadeEntryType ProjectCascadeEntryType { get; private set; }

		public int? ProjectTranslationProviderItemIndex { get; private set; }

		public ProjectCascadeEntryItem ProjectCascadeEntryItem => ProjectCascadeItem.CascadeEntryItems[ProjectCascadeEntryItemIndex];

		public ITranslationProviderItem TranslationProviderItem
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				if ((int)ProjectCascadeEntryType == 0)
				{
					return ProjectCascadeEntryItem.MainTranslationProviderItem;
				}
				return ProjectCascadeEntryItem.ProjectTranslationProviderItems[ProjectTranslationProviderItemIndex.Value];
			}
		}

		public ProjectCascadeEntryData(ProjectCascadeItem projectCascadeItem, int projectCascadeEntryItemIndex, ProjectCascadeEntryType projectCascadeEntryType, int? projectTranslationProviderItemIndex)
		{
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Invalid comparison between Unknown and I4
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			if (projectCascadeItem == null)
			{
				throw new ArgumentNullException("projectCascadeItem");
			}
			if (projectCascadeEntryItemIndex < 0 || projectCascadeEntryItemIndex >= projectCascadeItem.CascadeEntryItems.Count)
			{
				throw new ArgumentOutOfRangeException("projectCascadeEntryItemIndex");
			}
			if (projectTranslationProviderItemIndex.HasValue)
			{
				if ((int)projectCascadeEntryType == 0)
				{
					throw new ArgumentException("For a main translation provider, projectTranslationProviderItemIndex must equal null");
				}
				if (projectTranslationProviderItemIndex < 0 || projectTranslationProviderItemIndex >= projectCascadeItem.CascadeEntryItems[projectCascadeEntryItemIndex].ProjectTranslationProviderItems.Count)
				{
					throw new ArgumentOutOfRangeException("projectTranslationProviderItemIndex");
				}
			}
			else if ((int)projectCascadeEntryType == 1)
			{
				throw new ArgumentException("For a project translation provider, projectTranslationProviderItemIndex must not equal null");
			}
			ProjectCascadeItem = projectCascadeItem;
			ProjectCascadeEntryItemIndex = projectCascadeEntryItemIndex;
			ProjectCascadeEntryType = projectCascadeEntryType;
			ProjectTranslationProviderItemIndex = projectTranslationProviderItemIndex;
		}
	}
}
