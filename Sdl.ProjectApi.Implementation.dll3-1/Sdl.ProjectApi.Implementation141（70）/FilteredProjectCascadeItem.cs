using System.Collections.Generic;
using Sdl.Core.Globalization;
using Sdl.LanguagePlatform.Core;

namespace Sdl.ProjectApi.Implementation
{
	public class FilteredProjectCascadeItem : IFilteredProjectCascadeItem
	{
		private readonly ProjectCascadeItem _cascadeItem;

		private readonly IEnumerable<ILanguageDirection> _languageDirections;

		private readonly ITranslationMemoryInfoFactory _translationMemoryInfoFactory;

		public ProjectCascadeItem FilteredCascadeItem
		{
			get
			{
				ProjectCascadeItem val = _cascadeItem.Copy();
				for (int num = val.CascadeEntryItems.Count - 1; num >= 0; num--)
				{
					ProjectCascadeEntryItem val2 = val.CascadeEntryItems[num];
					if (MustBeFilteredOut(val2.MainTranslationProviderItem))
					{
						val.CascadeEntryItems.RemoveAt(num);
					}
					else
					{
						for (int num2 = val2.ProjectTranslationProviderItems.Count - 1; num2 >= 0; num2--)
						{
							if (MustBeFilteredOut(val2.ProjectTranslationProviderItems[num2]))
							{
								val2.ProjectTranslationProviderItems.RemoveAt(num2);
							}
						}
					}
				}
				return val;
			}
		}

		public FilteredProjectCascadeItem(ProjectCascadeItem cascadeItem, ITranslationMemoryInfoFactory translationMemoryInfoFactory, IEnumerable<ILanguageDirection> languageDirections)
		{
			_cascadeItem = cascadeItem;
			_languageDirections = languageDirections;
			_translationMemoryInfoFactory = translationMemoryInfoFactory;
		}

		private bool MustBeFilteredOut(ITranslationProviderItem translationProviderItem)
		{
			ITranslationMemoryInfo val = _translationMemoryInfoFactory.Create(translationProviderItem.Uri);
			bool flag = val != null && val.IsFileBasedTranslationMemory;
			bool? flag2 = ((val != null) ? new bool?(val.IsPasswordProtected()) : null);
			if (flag2 != true)
			{
				if (flag)
				{
					return !TranslationProviderMatchesLanguageDirections(val);
				}
				return false;
			}
			return flag2.Value;
		}

		private bool TranslationProviderMatchesLanguageDirections(ITranslationMemoryInfo translationMemoryInfo)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			if (_languageDirections == null)
			{
				return false;
			}
			foreach (ILanguageDirection languageDirection in _languageDirections)
			{
				LanguagePair val = new LanguagePair(CultureCode.op_Implicit(languageDirection.SourceLanguage.CultureInfo), CultureCode.op_Implicit(languageDirection.TargetLanguage.CultureInfo));
				if (translationMemoryInfo.SupportsLanguageDirection(val))
				{
					return true;
				}
			}
			return false;
		}
	}
}
