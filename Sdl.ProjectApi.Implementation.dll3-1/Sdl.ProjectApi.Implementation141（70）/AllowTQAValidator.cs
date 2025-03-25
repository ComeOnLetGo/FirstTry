using System;
using System.Security.Cryptography;
using System.Text;
using Sdl.Core.Settings;

namespace Sdl.ProjectApi.Implementation
{
	public static class AllowTQAValidator
	{
		public static bool ValidateProject(IProject project)
		{
			if (((IObjectWithSettings)project).Settings.ContainsSettingsGroup("TranslationQualityAssessmentSettings"))
			{
				ISettingsGroup settingsGroup = ((IObjectWithSettings)project).Settings.GetSettingsGroup("PackageLicenseInfo");
				Setting<string> setting = settingsGroup.GetSetting<string>("Grant");
				if (!string.IsNullOrWhiteSpace(setting.Value) && setting.Value.Contains("AllowTQA"))
				{
					Setting<string> setting2 = settingsGroup.GetSetting<string>("Hash");
					if (setting2 == null || string.IsNullOrWhiteSpace(setting2.Value) || !setting2.Value.Equals(HashProject(project.Guid.ToString()), StringComparison.Ordinal))
					{
						return false;
					}
				}
			}
			return true;
		}

		internal static string HashProject(string projectGuid)
		{
			string salt = string.Format("|{0}|{1}|{2}", "TranslationQualityAssessmentSettings", "Grant", "AllowTQA");
			return ComputeHash(projectGuid, salt);
		}

		private static string ComputeHash(string inputString, string salt)
		{
			byte[] bytes = Encoding.ASCII.GetBytes(inputString + salt);
			SHA256Managed sHA256Managed = new SHA256Managed();
			return Convert.ToBase64String(sHA256Managed.ComputeHash(bytes));
		}
	}
}
