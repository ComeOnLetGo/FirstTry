using System.Collections.Generic;
using Sdl.ProjectApi.Settings.SettingTypes;

namespace Sdl.ProjectApi.Implementation.LanguageCloud
{
	public interface IXmlProjectIdsReplacer
	{
		void ReplaceProjectIds(string projectFilePath, string newProjectId, Dictionary<string, string> fileIdAssociations);

		void ReplaceProjectIdForReturnPackage(string projectFilePath, MigrationData migrationData);
	}
}
