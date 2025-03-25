using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.ProjectApi.Implementation.ProjectOperationResults;

namespace Sdl.ProjectApi.Implementation.Operations
{
	public class RevertFilesToSdlXliffProjectOperation : AbstractProjectOperation
	{
		private static string _id => "RevertFilesToSdlXliffProjectOperation";

		public RevertFilesToSdlXliffProjectOperation(IProjectOperationSpecification specification)
			: base(specification)
		{
		}

		public override bool IsAllowed(IProject project, string operationId)
		{
			if (_id.Equals(operationId, StringComparison.OrdinalIgnoreCase))
			{
				return base.IsAllowed(project, operationId);
			}
			return false;
		}

		public override IProjectOperationResult Execute(IProject project, string operationId, object[] args)
		{
			ITranslatableFile[] files = ValidateArguments(args);
			RevertFiles(files);
			return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true);
		}

		private ITranslatableFile[] ValidateArguments(object[] args)
		{
			if (args == null)
			{
				throw new ArgumentNullException(StringResources.OperationNoArgumentMessage);
			}
			if (args.Length != 1)
			{
				throw new ArgumentException(StringResources.IncorrectArgumentNumberMessage);
			}
			if (!(args[0] is ITranslatableFile[] array))
			{
				throw new ArgumentException(StringResources.IncorrectArgumentMessage);
			}
			if (array.Length == 0)
			{
				throw new ArgumentException(StringResources.FilesOperationEmptyFilesArgumentMessage);
			}
			return array;
		}

		private void RevertFiles(ITranslatableFile[] files)
		{
			List<ITranslatableFile> revertFiles = GetRevertFiles(files);
			foreach (ITranslatableFile item in revertFiles)
			{
				item.RevertToSDLXLIFF();
			}
		}

		private List<ITranslatableFile> GetRevertFiles(IEnumerable<ITranslatableFile> files)
		{
			List<ITranslatableFile> list = new List<ITranslatableFile>();
			foreach (ITranslatableFile file in files)
			{
				IFileRevision mostRecentBilingualRevision = file.GetMostRecentBilingualRevision();
				if (mostRecentBilingualRevision != null)
				{
					list.Add(file);
					continue;
				}
				IMergedTranslatableFile[] mergedFileHistory = file.MergedFileHistory;
				if (mergedFileHistory.Any())
				{
					IMergedTranslatableFile lastParentFile = mergedFileHistory.Last();
					if (list.All((ITranslatableFile p) => ((IProjectFile)p).Guid != ((IProjectFile)lastParentFile).Guid))
					{
						list.Add((ITranslatableFile)(object)lastParentFile);
					}
				}
			}
			return list;
		}

		public override bool Equals(object obj)
		{
			return obj is RevertFilesToSdlXliffProjectOperation;
		}

		public override int GetHashCode()
		{
			return 332918;
		}
	}
}
