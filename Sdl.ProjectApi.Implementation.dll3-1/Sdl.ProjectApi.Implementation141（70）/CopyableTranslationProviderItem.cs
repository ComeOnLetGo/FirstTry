using System;
using System.IO;
using Sdl.Core.Globalization;
using Sdl.LanguagePlatform.TranslationMemoryApi;

namespace Sdl.ProjectApi.Implementation
{
	public class CopyableTranslationProviderItem : ICopyableTranslationProviderItem
	{
		private readonly ITranslationProviderItem _translationProviderItem;

		private readonly IProject _project;

		private readonly ILanguageDirection _languageDirection;

		private readonly IFileBasedTranslationMemoryHelper _fileBasedTranslationMemoryHelper;

		private readonly IFileOperations _fileOperations;

		private readonly ITranslationMemoryInfo _translationMemoryInfo;

		public bool CanBeCopied
		{
			get
			{
				if (_translationProviderItem != null && _translationProviderItem.Enabled && _translationMemoryInfo != null)
				{
					return _translationProviderItem.IsFileBasedTranslationMemory();
				}
				return false;
			}
		}

		private string TargetFilePath
		{
			get
			{
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Expected O, but got Unknown
				if (!CanBeCopied)
				{
					return null;
				}
				string filePath = _fileBasedTranslationMemoryHelper.GetFilePath(_translationProviderItem.Uri);
				string fileName = Path.GetFileName(filePath);
				Language val = new Language(_translationMemoryInfo.GetTargetLanguage());
				string path = string.Empty;
				if (_languageDirection == null && filePath.EndsWith(Path.Combine(((LanguageBase)val).IsoAbbreviation, fileName), StringComparison.InvariantCultureIgnoreCase))
				{
					path = ((LanguageBase)val).IsoAbbreviation;
				}
				string projectTmFolder = ProjectTranslationMemoryUtil.GetProjectTmFolder(_project, _languageDirection);
				return Path.Combine(projectTmFolder, path, fileName);
			}
		}

		public CopyableTranslationProviderItem(ITranslationProviderItem translationProviderItem, IProject project, ITranslationMemoryInfo translationMemoryInfo, ILanguageDirection languageDirection = null, IFileBasedTranslationMemoryHelper fileBasedTranslationMemoryHelper = null, IFileOperations fileOperations = null)
		{
			if (project == null)
			{
				throw new ArgumentNullException(StringResources.CopyableTranslationProviderItem_InvalidProject);
			}
			_translationProviderItem = translationProviderItem;
			_project = project;
			_languageDirection = languageDirection;
			_fileBasedTranslationMemoryHelper = fileBasedTranslationMemoryHelper ?? new FileBasedTranslationMemoryHelper();
			_translationMemoryInfo = translationMemoryInfo;
			_fileOperations = fileOperations ?? new FileOperations();
		}

		public void CopyTranslationProvider()
		{
			if (!CanBeCopied)
			{
				throw new InvalidOperationException(StringResources.CopyableTranslationProviderItem_InvalidTranslationProvider);
			}
			string filePath = _fileBasedTranslationMemoryHelper.GetFilePath(_translationProviderItem.Uri);
			if (!_fileOperations.FileExists(filePath))
			{
				_translationProviderItem.Enabled = false;
				return;
			}
			string targetFilePath = TargetFilePath;
			_fileOperations.CopyFile(filePath, targetFilePath);
			CopyPassword(filePath, targetFilePath);
			_translationProviderItem.Uri = _fileBasedTranslationMemoryHelper.GetUri(targetFilePath);
		}

		private void CopyPassword(string sourceFilePath, string targetFilePath)
		{
			ITranslationProviderCredentialStore translationProviderCredentialStore = ((IProjectConfiguration)_project).ProjectsProvider.Application.TranslationProviderCredentialStore;
			TranslationProviderCredential credential = translationProviderCredentialStore.GetCredential(_fileBasedTranslationMemoryHelper.GetUri(sourceFilePath));
			if (credential != null)
			{
				string credential2 = credential.Credential;
				if (credential2 != null)
				{
					translationProviderCredentialStore.AddCredential(_fileBasedTranslationMemoryHelper.GetUri(targetFilePath), credential);
				}
			}
		}
	}
}
