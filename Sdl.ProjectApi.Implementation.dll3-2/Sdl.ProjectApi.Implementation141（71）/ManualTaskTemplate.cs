using System;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ManualTaskTemplate : IManualTaskTemplate, ITaskTemplate
	{
		private TaskFileType[] _lazySupportedFileTypes;

		private TaskFileType? _lazyGeneratedFileType;

		private WorkflowStage? _lazyWorkflowStageType;

		public string Id => XmlManualTaskTemplate.Id;

		public string Name
		{
			get
			{
				string text = "";
				if (!string.IsNullOrEmpty(XmlManualTaskTemplate.Name) && XmlManualTaskTemplate.Name.StartsWith("ManualTask_"))
				{
					try
					{
						text = StringResources.ResourceManager.GetString(XmlManualTaskTemplate.Name);
					}
					catch
					{
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
				return XmlManualTaskTemplate.Name;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new ArgumentNullException("Name");
				}
				XmlManualTaskTemplate.Name = value;
			}
		}

		public string Description
		{
			get
			{
				string text = "";
				if (!string.IsNullOrEmpty(XmlManualTaskTemplate.Description) && XmlManualTaskTemplate.Description.StartsWith("ManualTask_"))
				{
					try
					{
						text = StringResources.ResourceManager.GetString(XmlManualTaskTemplate.Description);
					}
					catch
					{
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					return text;
				}
				return XmlManualTaskTemplate.Description;
			}
			set
			{
				XmlManualTaskTemplate.Description = value;
			}
		}

		public TaskFileType[] SupportedFileTypes
		{
			get
			{
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_004a: Expected I4, but got Unknown
				if (_lazySupportedFileTypes == null)
				{
					_lazySupportedFileTypes = (TaskFileType[])(object)new TaskFileType[XmlManualTaskTemplate.SupportedFileTypes.Count];
					for (int i = 0; i < XmlManualTaskTemplate.SupportedFileTypes.Count; i++)
					{
						_lazySupportedFileTypes[i] = (TaskFileType)(int)EnumConvert.ConvertTaskFileType(XmlManualTaskTemplate.SupportedFileTypes[i].FileType);
					}
				}
				return _lazySupportedFileTypes;
			}
		}

		public bool AllowMultiple => true;

		public bool RestrictedForTaskSequence => false;

		public TaskFileType GeneratedFileType
		{
			get
			{
				//IL_003e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				if (!_lazyGeneratedFileType.HasValue)
				{
					_lazyGeneratedFileType = (TaskFileType)(XmlManualTaskTemplate.GeneratedFileTypeSpecified ? ((int)EnumConvert.ConvertTaskFileType(XmlManualTaskTemplate.GeneratedFileType)) : 0);
				}
				return _lazyGeneratedFileType.Value;
			}
		}

		public string[] RequiredSettingsPageIds => new string[0];

		public Type[] RequiresBatchTaskSettingsPageTypes => new Type[0];

		public WorkflowStage WorkflowStage
		{
			get
			{
				//IL_002e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				if (!_lazyWorkflowStageType.HasValue)
				{
					_lazyWorkflowStageType = EnumConvert.ConvertWorkflowStageType(XmlManualTaskTemplate.WorkflowStageType);
				}
				return _lazyWorkflowStageType.Value;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				_lazyWorkflowStageType = value;
				XmlManualTaskTemplate.WorkflowStageType = EnumConvert.ConvertWorkflowStageType(value);
			}
		}

		internal Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate XmlManualTaskTemplate { get; }

		public bool IsLocallyExecutable => XmlManualTaskTemplate.IsLocallyExecutable;

		public bool IsImported => XmlManualTaskTemplate.IsImported;

		internal ManualTaskTemplate(Sdl.ProjectApi.Implementation.Xml.ManualTaskTemplate xmlManualTaskTemplate)
		{
			XmlManualTaskTemplate = xmlManualTaskTemplate;
		}

		public bool SupportsFileType(TaskFileType type)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return Array.IndexOf(SupportedFileTypes, type) != -1;
		}

		public bool SupportsFileTypes(TaskFileType[] types)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			foreach (TaskFileType type in types)
			{
				if (!SupportsFileType(type))
				{
					return false;
				}
			}
			return true;
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
