using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.Core.Settings;
using Sdl.FileTypeSupport.Framework;
using Sdl.FileTypeSupport.Framework.Integration;
using Sdl.FileTypeSupport.Framework.IntegrationApi;

namespace Sdl.ProjectApi.Implementation
{
	internal class FileTypeConfiguration : IFileTypeConfiguration
	{
		private readonly object _lockObject = new object();

		private readonly AbstractProjectConfiguration _projectConfiguration;

		private IFileTypeManager _filterManager;

		private IFileTypeManager _standardFilterManager;

		private FileTypeManagerConfiguration _fileTypeManagerConfiguration;

		private FileTypeManagerConfiguration _subContentFileTypeManagerConfiguration;

		public IFileTypeManager FilterManager
		{
			get
			{
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0037: Expected O, but got Unknown
				//IL_007e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0083: Unknown result type (might be due to invalid IL or missing references)
				lock (_lockObject)
				{
					if (_filterManager == null)
					{
						if (FileTypeManagerConfiguration.LoadConfigurationData())
						{
							_filterManager = (IFileTypeManager)new PocoFilterManager();
							foreach (FileTypeManagerConfigurationItem item in (IEnumerable<FileTypeManagerConfigurationItem>)FileTypeManagerConfiguration)
							{
								IFileTypeDefinition val = _filterManager.CreateFileTypeDefinition(item.FileTypeDefinitionId, item.Profile);
								if (val != null)
								{
									IFileTypeInformation fileTypeInformation = val.FileTypeInformation;
									FileTypeDefinitionId fileTypeDefinitionId = val.FileTypeInformation.FileTypeDefinitionId;
									fileTypeInformation.Hidden = OverrideHiddenFromPocoFilterManager(((FileTypeDefinitionId)(ref fileTypeDefinitionId)).Id, item.Hidden);
									val.FileTypeInformation.Enabled = item.Enabled;
									val.FileTypeInformation.Removed = item.Removed;
									_filterManager.AddFileTypeDefinition(val);
								}
							}
							foreach (string fdId in StandardFilterManager.AutoLoadedFileTypes)
							{
								int autoloadedFileTypeInsertIndex = GetAutoloadedFileTypeInsertIndex(fdId);
								if (autoloadedFileTypeInsertIndex > -1)
								{
									IFileTypeDefinition val2 = StandardFilterManager.FileTypeDefinitions.ToList().Find(delegate(IFileTypeDefinition fd)
									{
										//IL_0006: Unknown result type (might be due to invalid IL or missing references)
										//IL_000b: Unknown result type (might be due to invalid IL or missing references)
										FileTypeDefinitionId fileTypeDefinitionId4 = fd.FileTypeInformation.FileTypeDefinitionId;
										return ((FileTypeDefinitionId)(ref fileTypeDefinitionId4)).Id == fdId;
									});
									_filterManager.InsertFileTypeDefinition(autoloadedFileTypeInsertIndex, val2);
								}
							}
						}
						else
						{
							_filterManager = StandardFilterManager;
						}
						if (SubContentFileTypeManagerConfiguration.LoadConfigurationData())
						{
							foreach (FileTypeManagerConfigurationItem item2 in (IEnumerable<FileTypeManagerConfigurationItem>)SubContentFileTypeManagerConfiguration)
							{
								IFileTypeDefinition fileTypeDefinition = _filterManager.CreateFileTypeDefinition(item2.FileTypeDefinitionId, item2.Profile);
								if (fileTypeDefinition != null)
								{
									IEnumerable<IFileTypeDefinition> source = _filterManager.FileTypeDefinitions.Where(delegate(IFileTypeDefinition fd)
									{
										//IL_0006: Unknown result type (might be due to invalid IL or missing references)
										//IL_000b: Unknown result type (might be due to invalid IL or missing references)
										//IL_001e: Unknown result type (might be due to invalid IL or missing references)
										//IL_0023: Unknown result type (might be due to invalid IL or missing references)
										FileTypeDefinitionId fileTypeDefinitionId3 = fd.FileTypeInformation.FileTypeDefinitionId;
										string id2 = ((FileTypeDefinitionId)(ref fileTypeDefinitionId3)).Id;
										fileTypeDefinitionId3 = fileTypeDefinition.FileTypeInformation.FileTypeDefinitionId;
										return id2 == ((FileTypeDefinitionId)(ref fileTypeDefinitionId3)).Id;
									});
									if (source.Count() != 0)
									{
										_filterManager.RemoveFileTypeDefinition(source.First());
									}
									fileTypeDefinition.FileTypeInformation.Hidden = OverrideHiddenFromPocoFilterManager(item2.FileTypeDefinitionId, item2.Hidden);
									fileTypeDefinition.FileTypeInformation.Enabled = item2.Enabled;
									fileTypeDefinition.FileTypeInformation.Removed = item2.Removed;
									_filterManager.AddFileTypeDefinition(fileTypeDefinition);
								}
							}
						}
						IEnumerable<IFileTypeDefinition> enumerable = StandardFilterManager.FileTypeDefinitions.Where((IFileTypeDefinition fd) => fd.ComponentBuilder is ISubContentComponentBuilder);
						foreach (IFileTypeDefinition fd2 in enumerable)
						{
							IEnumerable<IFileTypeDefinition> source2 = _filterManager.FileTypeDefinitions.Where(delegate(IFileTypeDefinition fid)
							{
								//IL_0006: Unknown result type (might be due to invalid IL or missing references)
								//IL_000b: Unknown result type (might be due to invalid IL or missing references)
								//IL_001e: Unknown result type (might be due to invalid IL or missing references)
								//IL_0023: Unknown result type (might be due to invalid IL or missing references)
								FileTypeDefinitionId fileTypeDefinitionId2 = fid.FileTypeInformation.FileTypeDefinitionId;
								string id = ((FileTypeDefinitionId)(ref fileTypeDefinitionId2)).Id;
								fileTypeDefinitionId2 = fd2.FileTypeInformation.FileTypeDefinitionId;
								return id == ((FileTypeDefinitionId)(ref fileTypeDefinitionId2)).Id;
							});
							if (!source2.Any())
							{
								_filterManager.AddFileTypeDefinition(fd2);
							}
						}
					}
				}
				return _filterManager;
			}
		}

		public IFileTypeManager StandardFilterManager
		{
			get
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Expected O, but got Unknown
				//IL_0018: Expected O, but got Unknown
				IFileTypeManager obj = _standardFilterManager;
				if (obj == null)
				{
					PocoFilterManager val = new PocoFilterManager(true);
					IFileTypeManager val2 = (IFileTypeManager)val;
					_standardFilterManager = (IFileTypeManager)val;
					obj = val2;
				}
				return obj;
			}
		}

		public IFileTypeManagerConfiguration FileTypeManagerConfiguration => (IFileTypeManagerConfiguration)(object)(_fileTypeManagerConfiguration ?? (_fileTypeManagerConfiguration = new FileTypeManagerConfiguration(_projectConfiguration.Settings)));

		public IFileTypeManagerConfiguration SubContentFileTypeManagerConfiguration => (IFileTypeManagerConfiguration)(object)(_subContentFileTypeManagerConfiguration ?? (_subContentFileTypeManagerConfiguration = new FileTypeManagerConfiguration(_projectConfiguration.Settings, isSubContentSettings: true)));

		public event EventHandler FilterSettingsChanged;

		public FileTypeConfiguration(AbstractProjectConfiguration projectConfiguration)
		{
			_projectConfiguration = projectConfiguration;
		}

		internal void NotifyFilterManagerConfigurationChanged(object sender, EventArgs e)
		{
			Discard();
			DiscardSubContentInfo();
			OnFilterSettingsChanged();
		}

		private bool OverrideHiddenFromPocoFilterManager(string fileTypeDefinitionId, bool hidden)
		{
			IFileTypeDefinition val = StandardFilterManager.FileTypeDefinitions.FirstOrDefault(delegate(IFileTypeDefinition fd)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				FileTypeDefinitionId fileTypeDefinitionId2 = fd.FileTypeInformation.FileTypeDefinitionId;
				return ((FileTypeDefinitionId)(ref fileTypeDefinitionId2)).Id == fileTypeDefinitionId;
			});
			if (val == null)
			{
				return hidden;
			}
			return val.FileTypeInformation.Hidden;
		}

		private int GetAutoloadedFileTypeInsertIndex(string fileTypeDefinitionId)
		{
			int num = _filterManager.FileTypeDefinitions.ToList().FindIndex(delegate(IFileTypeDefinition fd)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				FileTypeDefinitionId fileTypeDefinitionId4 = fd.FileTypeInformation.FileTypeDefinitionId;
				return ((FileTypeDefinitionId)(ref fileTypeDefinitionId4)).Id == fileTypeDefinitionId;
			});
			if (num >= 0)
			{
				return -1;
			}
			List<IFileTypeDefinition> list = StandardFilterManager.FileTypeDefinitions.ToList();
			int num2 = list.FindIndex(delegate(IFileTypeDefinition fd)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				FileTypeDefinitionId fileTypeDefinitionId3 = fd.FileTypeInformation.FileTypeDefinitionId;
				return ((FileTypeDefinitionId)(ref fileTypeDefinitionId3)).Id == fileTypeDefinitionId;
			});
			for (int num3 = num2 - 1; num3 > 0; num3--)
			{
				IFileTypeDefinition testFd = list[num3];
				int num4 = _filterManager.FileTypeDefinitions.ToList().FindIndex(delegate(IFileTypeDefinition fd)
				{
					//IL_0006: Unknown result type (might be due to invalid IL or missing references)
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_001e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0023: Unknown result type (might be due to invalid IL or missing references)
					FileTypeDefinitionId fileTypeDefinitionId2 = fd.FileTypeInformation.FileTypeDefinitionId;
					string id = ((FileTypeDefinitionId)(ref fileTypeDefinitionId2)).Id;
					fileTypeDefinitionId2 = testFd.FileTypeInformation.FileTypeDefinitionId;
					return id == ((FileTypeDefinitionId)(ref fileTypeDefinitionId2)).Id;
				});
				if (num4 > -1)
				{
					return num4 + 1;
				}
			}
			return 1;
		}

		public void Save(ISettingsBundle settings)
		{
			FileTypeManagerConfiguration.SaveConfigurationData(settings);
		}

		public void Discard()
		{
			_filterManager = null;
			_standardFilterManager = null;
			FileTypeManagerConfiguration.ClearConfigurationData();
		}

		private void OnFilterSettingsChanged()
		{
			this.FilterSettingsChanged?.Invoke(this, EventArgs.Empty);
		}

		public void SaveSubContentInfo(ISettingsBundle settings)
		{
			SubContentFileTypeManagerConfiguration.SaveConfigurationData(settings);
		}

		public void DiscardSubContentInfo()
		{
			SubContentFileTypeManagerConfiguration.ClearConfigurationData();
		}
	}
}
