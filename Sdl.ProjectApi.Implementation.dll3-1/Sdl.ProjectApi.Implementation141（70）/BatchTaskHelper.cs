using Sdl.Core.PluginFramework;
using Sdl.ProjectApi.TaskImplementation;
using Sdl.ProjectAutomation.AutomaticTasks;

namespace Sdl.ProjectApi.Implementation
{
	internal static class BatchTaskHelper
	{
		public static TaskFileType BatchTaskFileTypeToTaskFileType(AutomaticTaskFileType fileType)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return (TaskFileType)fileType;
		}

		public static SupportedFileTypeAttribute BatchTaskSupportedFileTypeAttributeToSupportedFileTypeAttribute(AutomaticTaskSupportedFileTypeAttribute attribute)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			return new SupportedFileTypeAttribute(BatchTaskFileTypeToTaskFileType(attribute.FileType));
		}

		public static AutomaticTaskAttribute BatchTaskAttributeToAutomaticTaskAttribute(AutomaticTaskAttribute attribute)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			return new AutomaticTaskAttribute(((ExtensionAttribute)attribute).Id, ((ExtensionAttribute)attribute).Name, ((ExtensionAttribute)attribute).Description)
			{
				AllowMultiThreading = false,
				AllowMultiple = attribute.AllowMultiple,
				GeneratedFileType = BatchTaskFileTypeToTaskFileType(attribute.GeneratedFileType)
			};
		}
	}
}
