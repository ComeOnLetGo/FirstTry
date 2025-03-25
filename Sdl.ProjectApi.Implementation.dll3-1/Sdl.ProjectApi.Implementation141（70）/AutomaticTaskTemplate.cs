using System;
using System.IO;
using System.Linq;
using Sdl.Core.PluginFramework;
using Sdl.FileTypeSupport.Framework;
using Sdl.ProjectApi.TaskImplementation;
using Sdl.ProjectAutomation.AutomaticTasks;

namespace Sdl.ProjectApi.Implementation
{
	public class AutomaticTaskTemplate : IAutomaticTaskTemplate, ITaskTemplate, ITaskTemplateFileFilter
	{
		private readonly AutomaticTaskDescriptor _taskDescriptor;

		private TaskFileType[] _lazySupportedFileTypes;

		private string[] _lazyRequiredSettingsPageIds;

		private string[] _lazyFileExtensions;

		private FileTypeDefinitionId[] _lazyFileTypeDefinitions;

		private Type[] _lazyRequiresBatchTaskSettingsPageTypes;

		public string Id => _taskDescriptor.Id;

		public string Name
		{
			get
			{
				return _taskDescriptor.Name;
			}
			set
			{
				throw new NotSupportedException("You cannot set the name of an automatic task template.");
			}
		}

		public string Description
		{
			get
			{
				return _taskDescriptor.Description;
			}
			set
			{
				throw new NotSupportedException("You cannot set the description of an automatic task template.");
			}
		}

		public TaskFileType[] SupportedFileTypes
		{
			get
			{
				//IL_0030: Unknown result type (might be due to invalid IL or missing references)
				//IL_0036: Expected I4, but got Unknown
				if (_lazySupportedFileTypes == null)
				{
					SupportedFileTypeAttribute[] supportedFileTypesAttributes = _taskDescriptor.SupportedFileTypesAttributes;
					_lazySupportedFileTypes = (TaskFileType[])(object)new TaskFileType[supportedFileTypesAttributes.Length];
					for (int i = 0; i < supportedFileTypesAttributes.Length; i++)
					{
						_lazySupportedFileTypes[i] = (TaskFileType)(int)supportedFileTypesAttributes[i].FileType;
					}
				}
				return _lazySupportedFileTypes;
			}
		}

		public string[] RequiredSettingsPageIds
		{
			get
			{
				if (_lazyRequiredSettingsPageIds == null)
				{
					RequiresSettingsPageAttribute[] requiresSettingsPageAttributes = _taskDescriptor.RequiresSettingsPageAttributes;
					_lazyRequiredSettingsPageIds = new string[requiresSettingsPageAttributes.Length];
					for (int i = 0; i < requiresSettingsPageAttributes.Length; i++)
					{
						_lazyRequiredSettingsPageIds[i] = requiresSettingsPageAttributes[i].SettingsPageId;
					}
				}
				return _lazyRequiredSettingsPageIds;
			}
		}

		public Type[] RequiresBatchTaskSettingsPageTypes
		{
			get
			{
				if (_lazyRequiresBatchTaskSettingsPageTypes == null)
				{
					RequiresSettingsAttribute requiresBatchTaskSettingsAttribute = _taskDescriptor.RequiresBatchTaskSettingsAttribute;
					_lazyRequiresBatchTaskSettingsPageTypes = ((requiresBatchTaskSettingsAttribute == null) ? new Type[0] : new Type[1] { requiresBatchTaskSettingsAttribute.SettingsPageType });
				}
				return _lazyRequiresBatchTaskSettingsPageTypes;
			}
		}

		public TaskFileType GeneratedFileType => ExtensionAttribute.GeneratedFileType;

		public bool AllowMultiple => ExtensionAttribute.AllowMultiple;

		public bool AllowMultiThreading => ExtensionAttribute.AllowMultiThreading;

		public bool RestrictedForTaskSequence => ExtensionAttribute.RestrictedForTaskSequence;

		public AutomaticTaskType TaskType
		{
			get
			{
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				if (typeof(ISimpleTaskImplementation).IsAssignableFrom(_taskDescriptor.ExtensionType))
				{
					return (AutomaticTaskType)0;
				}
				if (typeof(IContentProcessingTaskImplementation).IsAssignableFrom(_taskDescriptor.ExtensionType))
				{
					return (AutomaticTaskType)1;
				}
				if (typeof(AbstractFileLevelAutomaticTask).IsAssignableFrom(_taskDescriptor.ExtensionType))
				{
					return (AutomaticTaskType)0;
				}
				if (typeof(AbstractFileContentProcessingAutomaticTask).IsAssignableFrom(_taskDescriptor.ExtensionType))
				{
					return (AutomaticTaskType)1;
				}
				throw new ProjectApiException("The task template does not implement one of the required interfaces");
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public bool IsLocallyExecutable => true;

		public string[] FileExtensionsAllowed
		{
			get
			{
				if (_lazyFileExtensions == null)
				{
					SupportedFileExtensionAttribute[] supportedFileExtensionAttributes = _taskDescriptor.SupportedFileExtensionAttributes;
					_lazyFileExtensions = (from x in supportedFileExtensionAttributes
						where !string.IsNullOrEmpty(x.Extension) && x.Extension.StartsWith(".")
						select x.Extension).ToArray();
				}
				return _lazyFileExtensions;
			}
		}

		public FileTypeDefinitionId[] FileTypeDefinitionsAllowed
		{
			get
			{
				if (_lazyFileTypeDefinitions == null)
				{
					SupportedFileExtensionAttribute[] supportedFileExtensionAttributes = _taskDescriptor.SupportedFileExtensionAttributes;
					_lazyFileTypeDefinitions = supportedFileExtensionAttributes.Where((SupportedFileExtensionAttribute x) => !string.IsNullOrEmpty(x.Extension) && !x.Extension.StartsWith(".")).Select((Func<SupportedFileExtensionAttribute, FileTypeDefinitionId>)((SupportedFileExtensionAttribute x) => new FileTypeDefinitionId(x.Extension))).ToArray();
				}
				return _lazyFileTypeDefinitions;
			}
		}

		internal IExtension Extension => _taskDescriptor.Extension;

		public AutomaticTaskAttribute ExtensionAttribute => _taskDescriptor.ExtensionAttribute;

		internal AutomaticTaskTemplate(IExtension extension)
			: this(new AutomaticTaskDescriptor(extension))
		{
			if (extension == null)
			{
				throw new ArgumentNullException("extension");
			}
		}

		internal AutomaticTaskTemplate(AutomaticTaskDescriptor taskDescriptor)
		{
			_taskDescriptor = taskDescriptor;
		}

		public bool SupportsFileType(TaskFileType taskFileType)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Array.IndexOf(SupportedFileTypes, taskFileType) != -1;
		}

		public virtual bool SupportsFileTypes(TaskFileType[] types)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			foreach (TaskFileType taskFileType in types)
			{
				if (!SupportsFileType(taskFileType))
				{
					return false;
				}
			}
			return true;
		}

		public IAbstractTaskImplementation CreateImplementation()
		{
			return _taskDescriptor.CreateInstance();
		}

		public bool SupportsFile(ITranslatableFile translatableFile)
		{
			if (FileExtensionsAllowed.Length == 0 && FileTypeDefinitionsAllowed.Length == 0)
			{
				return true;
			}
			if (FileExtensionsAllowed.Any((string extension) => SupportsFileExtension(translatableFile, extension)))
			{
				return true;
			}
			if (FileTypeDefinitionsAllowed.Any((FileTypeDefinitionId fileTypeDefinition) => SupportsFileDefinition(translatableFile, fileTypeDefinition)))
			{
				return true;
			}
			return false;
		}

		public bool SupportsFileExtension(ITranslatableFile translatableFile, string extension)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Invalid comparison between Unknown and I4
			if (string.IsNullOrEmpty(extension))
			{
				return false;
			}
			IMergedTranslatableFile val = (IMergedTranslatableFile)(object)((translatableFile is IMergedTranslatableFile) ? translatableFile : null);
			if (val != null)
			{
				return val.ChildFiles.Any(SupportsFile);
			}
			string path = (((int)translatableFile.TaskFileType == 3 && translatableFile.SourceLanguageFile != null) ? ((IProjectFile)translatableFile.SourceLanguageFile).Filename : ((IProjectFile)translatableFile).Filename);
			return string.Equals(Path.GetExtension(path), extension, StringComparison.InvariantCultureIgnoreCase);
		}

		public bool SupportsFileDefinition(ITranslatableFile translatableFile, FileTypeDefinitionId fileTypeDefinition)
		{
			IMergedTranslatableFile val = (IMergedTranslatableFile)(object)((translatableFile is IMergedTranslatableFile) ? translatableFile : null);
			if (val != null)
			{
				return val.ChildFiles.Any(SupportsFile);
			}
			return string.Equals(((FileTypeDefinitionId)(ref fileTypeDefinition)).Id, ((IProjectFile)translatableFile).FileTypeDefinitionId, StringComparison.InvariantCultureIgnoreCase);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
