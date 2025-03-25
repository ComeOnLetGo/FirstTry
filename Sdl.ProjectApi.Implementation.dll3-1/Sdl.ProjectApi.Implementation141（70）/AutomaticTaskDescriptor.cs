using System;
using System.Linq;
using Sdl.Core.PluginFramework;
using Sdl.ProjectApi.Implementation.TaskExecution;
using Sdl.ProjectApi.TaskImplementation;
using Sdl.ProjectAutomation.AutomaticTasks;

namespace Sdl.ProjectApi.Implementation
{
	internal class AutomaticTaskDescriptor
	{
		private string _id;

		private string _name;

		private string _description;

		private SupportedFileTypeAttribute[] _supportedFileTypesAttributes;

		private SupportedFileExtensionAttribute[] _supportedFileExtensionAttributes;

		private RequiresSettingsPageAttribute[] _requiresSettingsPageAttributes;

		private readonly IExtension _extension;

		public string Id
		{
			get
			{
				if (_id != null)
				{
					return _id;
				}
				if (_extension != null)
				{
					_id = _extension.ExtensionAttribute.Id;
				}
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		public string Name
		{
			get
			{
				if (_name != null)
				{
					return _name;
				}
				if (_extension != null)
				{
					_name = _extension.ExtensionAttribute.Name;
				}
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public string Description
		{
			get
			{
				if (_description != null)
				{
					return _description;
				}
				if (_extension != null)
				{
					_description = _extension.ExtensionAttribute.Description;
				}
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		public SupportedFileTypeAttribute[] SupportedFileTypesAttributes
		{
			get
			{
				if (_supportedFileTypesAttributes != null)
				{
					return _supportedFileTypesAttributes;
				}
				if (_extension != null)
				{
					_supportedFileTypesAttributes = _extension.GetAuxiliaryExtensionAttributes<SupportedFileTypeAttribute>();
					if (_supportedFileTypesAttributes == null || _supportedFileTypesAttributes.Length == 0)
					{
						AutomaticTaskSupportedFileTypeAttribute[] auxiliaryExtensionAttributes = _extension.GetAuxiliaryExtensionAttributes<AutomaticTaskSupportedFileTypeAttribute>();
						_supportedFileTypesAttributes = auxiliaryExtensionAttributes.Select((AutomaticTaskSupportedFileTypeAttribute x) => BatchTaskHelper.BatchTaskSupportedFileTypeAttributeToSupportedFileTypeAttribute(x)).ToArray();
					}
				}
				return _supportedFileTypesAttributes;
			}
			set
			{
				_supportedFileTypesAttributes = value;
			}
		}

		public SupportedFileExtensionAttribute[] SupportedFileExtensionAttributes
		{
			get
			{
				if (_supportedFileExtensionAttributes != null)
				{
					return _supportedFileExtensionAttributes;
				}
				if (_extension != null)
				{
					_supportedFileExtensionAttributes = _extension.GetAuxiliaryExtensionAttributes<SupportedFileExtensionAttribute>();
				}
				return _supportedFileExtensionAttributes;
			}
			set
			{
				_supportedFileExtensionAttributes = value;
			}
		}

		public RequiresSettingsPageAttribute[] RequiresSettingsPageAttributes
		{
			get
			{
				if (_requiresSettingsPageAttributes != null)
				{
					return _requiresSettingsPageAttributes;
				}
				if (_extension != null)
				{
					_requiresSettingsPageAttributes = _extension.GetAuxiliaryExtensionAttributes<RequiresSettingsPageAttribute>();
				}
				return _requiresSettingsPageAttributes;
			}
			set
			{
				_requiresSettingsPageAttributes = value;
			}
		}

		public RequiresSettingsAttribute RequiresBatchTaskSettingsAttribute
		{
			get
			{
				if (_extension != null)
				{
					RequiresSettingsAttribute[] auxiliaryExtensionAttributes = _extension.GetAuxiliaryExtensionAttributes<RequiresSettingsAttribute>();
					if (auxiliaryExtensionAttributes != null && auxiliaryExtensionAttributes.Length != 0)
					{
						return auxiliaryExtensionAttributes[0];
					}
				}
				return null;
			}
		}

		public Type ExtensionType
		{
			get
			{
				if (_extensionType != null)
				{
					return _extensionType;
				}
				if (_extension != null)
				{
					_extensionType = _extension.ExtensionType;
				}
				return _extensionType;
			}
			set
			{
				_extensionType = value;
			}
		}

		public IExtension Extension
		{
			get
			{
				if (_reportExtension != null)
				{
					return _reportExtension;
				}
				if (_extension != null)
				{
					_reportExtension = _extension;
				}
				return _reportExtension;
			}
			set
			{
				_reportExtension = value;
			}
		}

		public AutomaticTaskAttribute ExtensionAttribute
		{
			get
			{
				//IL_0035: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Expected O, but got Unknown
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Expected O, but got Unknown
				if (_extensionAttribute != null)
				{
					return _extensionAttribute;
				}
				if (_extension != null)
				{
					if (_extension.ExtensionAttribute is AutomaticTaskAttribute)
					{
						_extensionAttribute = (AutomaticTaskAttribute)_extension.ExtensionAttribute;
					}
					else
					{
						if (!(_extension.ExtensionAttribute is AutomaticTaskAttribute))
						{
							throw new Exception($"Invalid batch task attribute for {_extension.ExtensionAttribute.Name}");
						}
						_extensionAttribute = BatchTaskHelper.BatchTaskAttributeToAutomaticTaskAttribute((AutomaticTaskAttribute)_extension.ExtensionAttribute);
					}
				}
				return _extensionAttribute;
			}
			set
			{
				_extensionAttribute = value;
			}
		}

		private Type _extensionType { get; set; }

		private IExtension _reportExtension { get; set; }

		private AutomaticTaskAttribute _extensionAttribute { get; set; }

		public AutomaticTaskDescriptor()
		{
		}

		public AutomaticTaskDescriptor(IExtension extension)
		{
			_extension = extension;
		}

		public IAbstractTaskImplementation CreateInstance()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Expected O, but got Unknown
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Expected O, but got Unknown
			object obj = _extension.CreateInstance();
			if (obj is AbstractFileLevelAutomaticTask)
			{
				return (IAbstractTaskImplementation)(object)new SimpleTaskImplementationAdapter((AbstractFileLevelAutomaticTask)obj);
			}
			if (obj is AbstractFileContentProcessingAutomaticTask)
			{
				return (IAbstractTaskImplementation)(object)new ContentProcessingTaskImplementationAdapter((AbstractFileContentProcessingAutomaticTask)obj);
			}
			return (IAbstractTaskImplementation)obj;
		}
	}
}
