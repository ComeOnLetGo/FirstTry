using System;
using Sdl.ProjectApi.TermbaseApi;

namespace Sdl.ProjectApi.Implementation.Xml
{
	internal static class EnumConvert
	{
		private static readonly EnumConverter<TaskStatus, TaskStatus> _taskStatusConverter = new EnumConverter<TaskStatus, TaskStatus>();

		private static readonly EnumConverter<FileRole, FileRole> _fileRoleConverter = new EnumConverter<FileRole, FileRole>();

		private static readonly EnumConverter<PackageStatus, PackageStatus> _packageStatusConverter = new EnumConverter<PackageStatus, PackageStatus>();

		private static readonly EnumConverter<PackageType, PackageType> _packageTypeConverter = new EnumConverter<PackageType, PackageType>();

		private static readonly EnumConverter<MergeState, MergeState> _mergeStateConverter = new EnumConverter<MergeState, MergeState>();

		private static readonly EnumConverter<MessageLevel, MessageLevel> _messageLevelConverter = new EnumConverter<MessageLevel, MessageLevel>();

		private static readonly EnumConverter<ValueStatus, ValueStatus> _valueStatusConverter = new EnumConverter<ValueStatus, ValueStatus>();

		private static readonly EnumConverter<TaskFileType, TaskFileType> _taskFileTypeConverter = new EnumConverter<TaskFileType, TaskFileType>();

		private static readonly EnumConverter<WorkflowStage, WorkflowStageType> _workflowStageTypeConverter = new EnumConverter<WorkflowStage, WorkflowStageType>();

		private static readonly EnumConverter<EmailType, EmailType> _emailTypeConverter = new EnumConverter<EmailType, EmailType>();

		private static readonly EnumConverter<TermbaseSearchOrder, TermbaseSearchOrder> _termbaseSearchOrderConverter = new EnumConverter<TermbaseSearchOrder, TermbaseSearchOrder>();

		private static readonly EnumConverter<FilePurpose, TaskFilePurpose> _filePurposeConverter = new EnumConverter<FilePurpose, TaskFilePurpose>();

		public static TaskStatus ConvertTaskStatus(TaskStatus xmlTaskStatus)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _taskStatusConverter.ConvertEnumValue(xmlTaskStatus);
		}

		public static TaskStatus ConvertTaskStatus(TaskStatus taskStatus)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _taskStatusConverter.ConvertEnumValue(taskStatus);
		}

		public static FileRole ConvertFileRole(FileRole fileRole)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _fileRoleConverter.ConvertEnumValue(fileRole);
		}

		public static FileRole ConvertFileRole(FileRole fileRole)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _fileRoleConverter.ConvertEnumValue(fileRole);
		}

		public static PackageStatus ConvertPackageStatus(PackageStatus s)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			return (PackageStatus)Enum.Parse(typeof(PackageStatus), s.ToString());
		}

		public static PackageStatus ConvertPackageStatus(PackageStatus s)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _packageStatusConverter.ConvertEnumValue(s);
		}

		public static PackageType ConvertPackageType(PackageType t)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _packageTypeConverter.ConvertEnumValue(t);
		}

		public static PackageType ConvertPackageType(PackageType t)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _packageTypeConverter.ConvertEnumValue(t);
		}

		public static MergeState ConvertMergeState(MergeState t)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _mergeStateConverter.ConvertEnumValue(t);
		}

		public static MergeState ConvertMergeState(MergeState t)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _mergeStateConverter.ConvertEnumValue(t);
		}

		public static MessageLevel ConvertMessageLevel(MessageLevel t)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _messageLevelConverter.ConvertEnumValue(t);
		}

		public static MessageLevel ConvertMessageLevel(MessageLevel t)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _messageLevelConverter.ConvertEnumValue(t);
		}

		public static ValueStatus ConvertValueStatus(ValueStatus t)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _valueStatusConverter.ConvertEnumValue(t);
		}

		public static ValueStatus ConvertValueStatus(ValueStatus t)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _valueStatusConverter.ConvertEnumValue(t);
		}

		public static TaskFileType ConvertTaskFileType(TaskFileType t)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _taskFileTypeConverter.ConvertEnumValue(t);
		}

		public static TaskFileType ConvertTaskFileType(TaskFileType t)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _taskFileTypeConverter.ConvertEnumValue(t);
		}

		public static WorkflowStage ConvertWorkflowStageType(WorkflowStageType t)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _workflowStageTypeConverter.ConvertEnumValue(t);
		}

		public static WorkflowStageType ConvertWorkflowStageType(WorkflowStage t)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _workflowStageTypeConverter.ConvertEnumValue(t);
		}

		public static EmailType ConvertEmailType(EmailType t)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _emailTypeConverter.ConvertEnumValue(t);
		}

		public static EmailType ConvertEmailType(EmailType t)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _emailTypeConverter.ConvertEnumValue(t);
		}

		public static TermbaseSearchOrder ConvertTermbaseSearchOrder(TermbaseSearchOrder t)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _termbaseSearchOrderConverter.ConvertEnumValue(t);
		}

		public static TermbaseSearchOrder ConvertTermbaseSearchOrder(TermbaseSearchOrder t)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _termbaseSearchOrderConverter.ConvertEnumValue(t);
		}

		public static TaskFilePurpose ConvertFilePurpose(FilePurpose filePurpose)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return _filePurposeConverter.ConvertEnumValue(filePurpose);
		}

		public static FilePurpose ConvertFilePurpose(TaskFilePurpose taskFilePurpose)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return _filePurposeConverter.ConvertEnumValue(taskFilePurpose);
		}

		public static ProjectStatus ConvertProjectStatus(ProjectStatus projectStatus)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			return (ProjectStatus)(projectStatus switch
			{
				ProjectStatus.Pending => 1, 
				ProjectStatus.Started => 2, 
				ProjectStatus.Completed => 3, 
				ProjectStatus.Archived => 4, 
				_ => throw new ProjectApiException("Unknown project status: " + projectStatus), 
			});
		}

		public static ProjectStatus ConvertProjectStatus(ProjectStatus projectStatus)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected I4, but got Unknown
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			return (projectStatus - 1) switch
			{
				0 => ProjectStatus.Pending, 
				1 => ProjectStatus.Started, 
				2 => ProjectStatus.Completed, 
				3 => ProjectStatus.Archived, 
				_ => throw new ProjectApiException("Unknown project status: " + ((object)(ProjectStatus)(ref projectStatus)).ToString()), 
			};
		}
	}
}
