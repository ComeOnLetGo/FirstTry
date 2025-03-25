using System;
using System.Linq;
using Sdl.ProjectApi.Implementation.ProjectOperationResults;

namespace Sdl.ProjectApi.Implementation.Operations
{
	public class CreateProjectPackageOperation : AbstractProjectOperation
	{
		private readonly IProjectPathUtil _projectPathUtil;

		private static string _id => "CreateProjectPackageOperation";

		public CreateProjectPackageOperation(IProjectOperationSpecification specification, IProjectPathUtil projectPathUtil)
			: base(specification)
		{
			_projectPathUtil = projectPathUtil;
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
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			if (args == null)
			{
				throw new ArgumentNullException(StringResources.OperationNoArgumentMessage);
			}
			if (args.Length != 4)
			{
				throw new ArgumentException(StringResources.IncorrectArgumentNumberMessage);
			}
			if (!(args[0] is IManualTask[] array) || array.Length == 0)
			{
				throw new ArgumentNullException("IManualTask");
			}
			object obj = args[3];
			ProjectPackageCreationOptions val = (ProjectPackageCreationOptions)((obj is ProjectPackageCreationOptions) ? obj : null);
			if (val == null)
			{
				throw new ArgumentNullException("ProjectPackageCreationOptions");
			}
			string packageName = args[1] as string;
			string comment = args[2] as string;
			Guid originalProjectGuid = ((ITaskFile)array[0].Files[0]).ProjectFile.Project.Guid;
			if (array.SelectMany((IManualTask task) => task.Files.Cast<ITaskFile>()).Any((ITaskFile file) => !file.ProjectFile.Project.Guid.Equals(originalProjectGuid)))
			{
				throw new ProjectApiException("A package can only contain files from one project");
			}
			ProjectPackageCreation result = new ProjectPackageCreation(project, array, packageName, comment, val, ((IProjectConfiguration)project).ProjectsProvider.PackageInitializer, _projectPathUtil);
			return (IProjectOperationResult)(object)new SimpleBooleanProjectOperationResult(result: true)
			{
				Result = result
			};
		}

		public override bool Equals(object obj)
		{
			return obj is CreateProjectPackageOperation;
		}

		public override int GetHashCode()
		{
			return 57384980;
		}
	}
}
