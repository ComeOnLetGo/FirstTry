using System;
using System.Collections.Generic;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class ManualTask : ScheduledTask, IManualCollaborativeTask, IManualTask, IScheduledTask, ITaskBase, ICollaborativeTask<IManualTaskFile>
	{
		private IUser _lazyAssignedBy;

		private IUser _lazyAssignedTo;

		public override ITaskTemplate[] TaskTemplates => (ITaskTemplate[])(object)new ITaskTemplate[1] { (ITaskTemplate)ManualTaskTemplate };

		public IManualTaskTemplate ManualTaskTemplate => base.ProjectImpl.GetManualTaskTemplate(XmlTask.TaskTemplateIds[0]);

		public IUser AssignedTo
		{
			get
			{
				if (_lazyAssignedTo == null)
				{
					_lazyAssignedTo = base.ProjectImpl.GetUserById(XmlTask.AssignedTo);
				}
				return _lazyAssignedTo;
			}
			set
			{
				_lazyAssignedTo = value;
				Sdl.ProjectApi.Implementation.Xml.ManualTask xmlTask = XmlTask;
				IUser lazyAssignedTo = _lazyAssignedTo;
				xmlTask.AssignedTo = ((lazyAssignedTo != null) ? lazyAssignedTo.UserId : null);
			}
		}

		public IUser AssignedBy
		{
			get
			{
				if (_lazyAssignedBy == null)
				{
					_lazyAssignedBy = base.ProjectImpl.GetUserById(XmlTask.AssignedBy);
				}
				return _lazyAssignedBy;
			}
			set
			{
				_lazyAssignedBy = value;
				XmlTask.AssignedBy = ((_lazyAssignedBy != null) ? _lazyAssignedBy.UserId : null);
			}
		}

		public DateTime AssignedAt
		{
			get
			{
				if (!XmlTask.AssignedAtSpecified)
				{
					return DateTime.MaxValue;
				}
				return XmlTask.AssignedAt;
			}
		}

		public DateTime DueDate
		{
			get
			{
				if (!XmlTask.DueDateSpecified)
				{
					return DateTime.MaxValue;
				}
				return XmlTask.DueDate.ToLocalTime();
			}
			set
			{
				XmlTask.DueDateSpecified = value != DateTime.MaxValue;
				if (XmlTask.DueDateSpecified)
				{
					XmlTask.DueDate = value.ToUniversalTime();
				}
				else
				{
					XmlTask.DueDate = DateTime.MaxValue;
				}
				(((IProjectConfiguration)base.Project).ProjectsProvider as ProjectsProvider).AddOrUpdateManualTaskIndexEntry((IManualTask)(object)this);
			}
		}

		public new string Comment
		{
			get
			{
				return XmlTask.Comment;
			}
			set
			{
				XmlTask.Comment = value;
			}
		}

		public bool IsLocallyExecutable => ((ITaskTemplate)ManualTaskTemplate).IsLocallyExecutable;

		internal new Sdl.ProjectApi.Implementation.Xml.ManualTask XmlTask { get; }

		public new IManualTaskFile[] Files => Array.ConvertAll(base.Files, (Converter<ITaskFile, IManualTaskFile>)((ITaskFile tf) => (IManualTaskFile)tf));

		public new IManualTaskFile[] TranslatableFiles => Array.ConvertAll(base.TranslatableFiles, (Converter<ITaskFile, IManualTaskFile>)((ITaskFile tf) => (IManualTaskFile)tf));

		internal ManualTask(IProject project, Sdl.ProjectApi.Implementation.Xml.ManualTask xmlManualTask, TaskFileFactory taskFileBuilder, IProjectPathUtil projectPathUtil)
			: base(project, xmlManualTask, taskFileBuilder, projectPathUtil)
		{
			XmlTask = xmlManualTask;
		}

		public void Assign(IUser user, string comment)
		{
			Assign(base.Project.CurrentUser, user, comment);
		}

		public void Assign(IUser assignedBy, IUser assignedTo, string comment)
		{
			XmlTask.AssignedTo = assignedTo.UserId;
			((IProjectConfiguration)base.Project).AddUserToCache(assignedTo);
			if (assignedBy != null)
			{
				XmlTask.AssignedBy = assignedBy.UserId;
				((IProjectConfiguration)base.Project).AddUserToCache(assignedBy);
			}
			else
			{
				AddPlaceHolderUser();
			}
			XmlTask.AssignedAt = DateTime.UtcNow;
			XmlTask.AssignedAtSpecified = true;
			XmlTask.Comment = comment;
			XmlTask.Status = TaskStatus.Assigned;
		}

		public void UpdateFromManualTask(IManualTask sourceTask)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Invalid comparison between Unknown and I4
			XmlTask.Status = EnumConvert.ConvertTaskStatus(((IScheduledTask)sourceTask).Status);
			if (sourceTask.AssignedBy != null)
			{
				AssignedBy = sourceTask.AssignedBy;
			}
			if (sourceTask.AssignedTo != null)
			{
				AssignedTo = sourceTask.AssignedTo;
			}
			if ((int)((IScheduledTask)sourceTask).Status == 9)
			{
				XmlTask.CompletedAtSpecified = true;
				XmlTask.CompletedAt = ((IScheduledTask)sourceTask).CompletedAt.ToUniversalTime();
			}
			XmlTask.PercentComplete = ((IScheduledTask)sourceTask).PercentComplete;
		}

		public void UpdateTaskStatus(TaskStatus newTaskStatus)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			XmlTask.Status = EnumConvert.ConvertTaskStatus(newTaskStatus);
		}

		public override string ToString()
		{
			return TaskTemplates[0].Name;
		}

		public new IManualTaskFile AddWorkFile(ILocalizableFile localizableFile)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			return (IManualTaskFile)base.AddWorkFile(localizableFile);
		}

		IManualTaskFile ICollaborativeTask<IManualTaskFile>.AddWorkFile(ILocalizableFile localizableFile, ITaskFile parentTaskFile)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			return (IManualTaskFile)AddWorkFile(localizableFile, parentTaskFile);
		}

		public new IManualTaskFile AddReferenceFile(IProjectFile projectFile)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Expected O, but got Unknown
			return (IManualTaskFile)base.AddReferenceFile(projectFile);
		}

		public new IManualTaskFile AddFile(IProjectFile file, FilePurpose purpose)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			return (IManualTaskFile)base.AddFile(file, purpose);
		}

		IManualTaskFile ICollaborativeTask<IManualTaskFile>.AddFile(IProjectFile file, FilePurpose purpose, ITaskFile parentTaskFile)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Expected O, but got Unknown
			return (IManualTaskFile)AddFile(file, purpose, parentTaskFile);
		}

		IManualTaskFile ICollaborativeTask<IManualTaskFile>.AddFile(IProjectFile file, FilePurpose purpose, ITaskFile parentTaskFile, Guid taskFileId)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			return (IManualTaskFile)AddFile(file, purpose, parentTaskFile, taskFileId);
		}

		public void RemoveFile(IManualTaskFile taskFile)
		{
			base.RemoveFile((ITaskFile)(object)taskFile);
		}

		public void RemoveFiles(IEnumerable<IManualTaskFile> taskFiles)
		{
			List<ITaskFile> list = new List<ITaskFile>();
			foreach (IManualTaskFile taskFile in taskFiles)
			{
				list.Add((ITaskFile)(object)taskFile);
			}
			RemoveFiles(list);
		}

		private void AddPlaceHolderUser()
		{
			IUser val = ((IProjectConfiguration)base.Project).ProjectsProvider.UserProvider.GetUserById("N\\A");
			if (val == null)
			{
				val = ((IProjectConfiguration)base.Project).ProjectsProvider.UserProvider.CreateUser("N\\A", "N\\A");
			}
			XmlTask.AssignedBy = val.UserId;
			XmlTask.CreatedBy = val.UserId;
		}

		public void SetStatus(TaskStatus status)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			base.Status = status;
		}
	}
}
