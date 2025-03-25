using System;
using System.Collections.Generic;
using System.IO;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	internal class ProjectCascadeItemValidator
	{
		private readonly IProjectConfiguration _configuration;

		private readonly IServerEvents _serverEvents;

		public ProjectCascadeItemValidator(IProjectConfiguration configuration, IServerEvents serverEvents)
		{
			_configuration = configuration;
			_serverEvents = serverEvents;
		}

		public void Validate(ProjectCascadeItem cascadeItem)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			cascadeItem.BeginEdit();
			bool flag = false;
			IList<ProjectCascadeEntryItem> cascadeEntryItems = cascadeItem.CascadeEntryItems;
			for (int num = cascadeEntryItems.Count - 1; num >= 0; num--)
			{
				ProjectCascadeEntryItem val = cascadeEntryItems[num];
				ITranslationProviderItem mainTranslationProviderItem = val.MainTranslationProviderItem;
				flag |= ValidateLocalTranslationMemoryPath(mainTranslationProviderItem);
				foreach (TranslationProviderItem projectTranslationProviderItem in val.ProjectTranslationProviderItems)
				{
					TranslationProviderItem translationProviderItem = projectTranslationProviderItem;
					flag |= ValidateLocalTranslationMemoryPath((ITranslationProviderItem)(object)translationProviderItem);
				}
			}
			if (flag)
			{
				cascadeItem.EndEdit();
			}
			else
			{
				cascadeItem.CancelEdit();
			}
		}

		private bool ValidateLocalTranslationMemoryPath(ITranslationProviderItem translationProviderItem)
		{
			if (translationProviderItem.Enabled && FileBasedTranslationMemory.IsFileBasedTranslationMemory(translationProviderItem.Uri) && !FileBasedTranslationMemoryExists(translationProviderItem))
			{
				if (!UpdateFileBasedTranslationMemoryPath(translationProviderItem))
				{
					translationProviderItem.Enabled = false;
				}
				return true;
			}
			return false;
		}

		private bool FileBasedTranslationMemoryExists(ITranslationProviderItem fileBasedTranslationMemoryItem)
		{
			string absoluteFilePath = GetAbsoluteFilePath(FileBasedTranslationMemory.GetFileBasedTranslationMemoryFilePath(fileBasedTranslationMemoryItem.Uri));
			return File.Exists(absoluteFilePath);
		}

		private bool UpdateFileBasedTranslationMemoryPath(ITranslationProviderItem fileBasedTranslationMemoryItem)
		{
			string absoluteFilePath = GetAbsoluteFilePath(FileBasedTranslationMemory.GetFileBasedTranslationMemoryFilePath(fileBasedTranslationMemoryItem.Uri));
			string localTranslationMemoryLocation = _serverEvents.GetLocalTranslationMemoryLocation(_configuration, absoluteFilePath);
			if (localTranslationMemoryLocation != null && File.Exists(localTranslationMemoryLocation))
			{
				fileBasedTranslationMemoryItem.Uri = GetFileBasedTranslationMemoryUri(localTranslationMemoryLocation);
				return true;
			}
			return false;
		}

		private string GetAbsoluteFilePath(string relativeFilePath)
		{
			IProjectConfiguration configuration = _configuration;
			IRelativePathManager val = (IRelativePathManager)(object)((configuration is IRelativePathManager) ? configuration : null);
			if (val != null)
			{
				return val.MakeAbsolutePath(relativeFilePath);
			}
			return relativeFilePath;
		}

		private Uri GetFileBasedTranslationMemoryUri(string relativeFilePath)
		{
			return FileBasedTranslationMemory.GetFileBasedTranslationMemoryUri(relativeFilePath);
		}
	}
}
