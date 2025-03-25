using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Sdl.ProjectApi.Implementation
{
	public class ProjectFilePathValidator : List<string>, IProjectFilePathValidator
	{
		private readonly IProjectPathUtil _projectPathUtil;

		private readonly string _localDataFolder;

		public ProjectFilePathValidator(IProjectPathUtil projectPathUtil, string localDataFolder)
		{
			_projectPathUtil = projectPathUtil;
			_localDataFolder = localDataFolder;
		}

		public bool Validate(string fileItem)
		{
			fileItem = _projectPathUtil.MakeAbsolutePath(fileItem, _localDataFolder);
			if (!File.Exists(fileItem))
			{
				return false;
			}
			if (FindIndex((string f) => string.Compare(f, fileItem, ignoreCase: true, CultureInfo.InvariantCulture) == 0) != -1)
			{
				return false;
			}
			Add(fileItem);
			return true;
		}
	}
}
