using System.Collections;
using System.Collections.Generic;
using Sdl.Core.Settings;
using Sdl.FileTypeSupport.Framework;
using Sdl.FileTypeSupport.Framework.IntegrationApi;

namespace Sdl.ProjectApi.Implementation
{
	public class FileTypeManagerConfiguration : IFileTypeManagerConfiguration, IList<FileTypeManagerConfigurationItem>, ICollection<FileTypeManagerConfigurationItem>, IEnumerable<FileTypeManagerConfigurationItem>, IEnumerable
	{
		private const string FileTypeManagerConfigurationSettingsGroup = "FileTypeManagerConfigurationSettingsGroup";

		private const string FileTypeManagerConfigurationSettings = "FileTypeManagerConfigurationSettings";

		private const string FileTypeManagerSubContentConfigurationSettings = "FileTypeManagerSubContentConfigurationSettings";

		private ISettingsBundle _settingsBundle;

		private FileTypeManagerConfigurationList _fileTypeConfigurationManagerList;

		private readonly bool _isSubContentSettings;

		public FileTypeManagerConfigurationItem this[int index]
		{
			get
			{
				return ((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList)[index];
			}
			set
			{
				((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList)[index] = value;
			}
		}

		public int Count => ((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).Count;

		bool ICollection<FileTypeManagerConfigurationItem>.IsReadOnly => ((ICollection<FileTypeManagerConfigurationItem>)_fileTypeConfigurationManagerList).IsReadOnly;

		public FileTypeManagerConfiguration(ISettingsBundle settingsBundle)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			_settingsBundle = settingsBundle;
			_fileTypeConfigurationManagerList = new FileTypeManagerConfigurationList();
		}

		public FileTypeManagerConfiguration(ISettingsBundle settingsBundle, bool isSubContentSettings)
			: this(settingsBundle)
		{
			_isSubContentSettings = isSubContentSettings;
		}

		public bool LoadConfigurationData()
		{
			if (_settingsBundle == null)
			{
				return false;
			}
			ISettingsGroup settingsGroup = _settingsBundle.GetSettingsGroup("FileTypeManagerConfigurationSettingsGroup");
			if (_isSubContentSettings)
			{
				if (settingsGroup.ContainsSetting("FileTypeManagerSubContentConfigurationSettings"))
				{
					_fileTypeConfigurationManagerList = settingsGroup.GetSetting<FileTypeManagerConfigurationList>("FileTypeManagerSubContentConfigurationSettings").Value;
					return true;
				}
			}
			else if (settingsGroup.ContainsSetting("FileTypeManagerConfigurationSettings"))
			{
				_fileTypeConfigurationManagerList = settingsGroup.GetSetting<FileTypeManagerConfigurationList>("FileTypeManagerConfigurationSettings").Value;
				return true;
			}
			return false;
		}

		public void SaveConfigurationData(ISettingsBundle settings)
		{
			if (settings != null || (_settingsBundle != null && Count != 0))
			{
				if (settings != null)
				{
					_settingsBundle = settings;
				}
				ISettingsGroup settingsGroup = _settingsBundle.GetSettingsGroup("FileTypeManagerConfigurationSettingsGroup");
				if (_isSubContentSettings)
				{
					settingsGroup.GetSetting<FileTypeManagerConfigurationList>("FileTypeManagerSubContentConfigurationSettings").Value = _fileTypeConfigurationManagerList;
				}
				else
				{
					settingsGroup.GetSetting<FileTypeManagerConfigurationList>("FileTypeManagerConfigurationSettings").Value = _fileTypeConfigurationManagerList;
				}
			}
		}

		public void ClearConfigurationData()
		{
			Clear();
			if (_settingsBundle != null)
			{
				ISettingsGroup settingsGroup = _settingsBundle.GetSettingsGroup("FileTypeManagerConfigurationSettingsGroup");
				if (_isSubContentSettings)
				{
					settingsGroup.RemoveSetting("FileTypeManagerSubContentConfigurationSettings");
				}
				else
				{
					settingsGroup.RemoveSetting("FileTypeManagerConfigurationSettings");
				}
			}
		}

		public void PopulateFromFileTypeManager(IFileTypeManager fileTypeManager)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Expected O, but got Unknown
			ClearConfigurationData();
			string empty = string.Empty;
			IFileTypeDefinition[] fileTypeDefinitions = fileTypeManager.FileTypeDefinitions;
			foreach (IFileTypeDefinition val in fileTypeDefinitions)
			{
				FileTypeDefinitionId fileTypeDefinitionId;
				if ((int)val.CustomizationLevel == 2)
				{
					fileTypeDefinitionId = val.ComponentBuilder.BuildFileTypeInformation(string.Empty).FileTypeDefinitionId;
					empty = ((FileTypeDefinitionId)(ref fileTypeDefinitionId)).Id;
				}
				else
				{
					fileTypeDefinitionId = val.FileTypeInformation.FileTypeDefinitionId;
					empty = ((FileTypeDefinitionId)(ref fileTypeDefinitionId)).Id;
				}
				FileTypeManagerConfigurationItem item = new FileTypeManagerConfigurationItem(empty, val.FileTypeInformation.Enabled, val.FileTypeInformation.Hidden, val.FileTypeInformation.Removed, CreateProfile(val));
				Add(item);
			}
		}

		private static FileTypeProfile CreateProfile(IFileTypeDefinition fileTypeDefinition)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Expected O, but got Unknown
			FileTypeProfile val = null;
			if ((int)fileTypeDefinition.CustomizationLevel != 0)
			{
				FileTypeProfile val2 = new FileTypeProfile
				{
					Description = fileTypeDefinition.FileTypeInformation.Description.Content,
					Expression = fileTypeDefinition.FileTypeInformation.Expression,
					FileDialogWildcardExpression = fileTypeDefinition.FileTypeInformation.FileDialogWildcardExpression
				};
				FileTypeDefinitionId fileTypeDefinitionId = fileTypeDefinition.FileTypeInformation.FileTypeDefinitionId;
				val2.FileTypeDefinitionId = ((FileTypeDefinitionId)(ref fileTypeDefinitionId)).Id;
				val2.FileTypeDocumentName = fileTypeDefinition.FileTypeInformation.FileTypeDocumentName.Content;
				val2.FileTypeDocumentsName = fileTypeDefinition.FileTypeInformation.FileTypeDocumentsName.Content;
				val2.FileTypeName = fileTypeDefinition.FileTypeInformation.FileTypeName.Content;
				val = val2;
				if (fileTypeDefinition.FileTypeInformation.Icon != null && !fileTypeDefinition.FileTypeInformation.Icon.IsEmbedded)
				{
					val.IconContent = fileTypeDefinition.FileTypeInformation.Icon.Content;
				}
			}
			return val;
		}

		public void Add(FileTypeManagerConfigurationItem item)
		{
			((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).Add(item);
		}

		public void Clear()
		{
			((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).Clear();
		}

		public bool Contains(FileTypeManagerConfigurationItem item)
		{
			return ((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).Contains(item);
		}

		public void CopyTo(FileTypeManagerConfigurationItem[] array, int arrayIndex)
		{
			((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).CopyTo(array, arrayIndex);
		}

		public IEnumerator<FileTypeManagerConfigurationItem> GetEnumerator()
		{
			return ((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_fileTypeConfigurationManagerList).GetEnumerator();
		}

		public int IndexOf(FileTypeManagerConfigurationItem item)
		{
			return ((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).IndexOf(item);
		}

		public void Insert(int index, FileTypeManagerConfigurationItem item)
		{
			((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).Insert(index, item);
		}

		public bool Remove(FileTypeManagerConfigurationItem item)
		{
			return ((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).Remove(item);
		}

		public void RemoveAt(int index)
		{
			((List<FileTypeManagerConfigurationItem>)(object)_fileTypeConfigurationManagerList).RemoveAt(index);
		}
	}
}
