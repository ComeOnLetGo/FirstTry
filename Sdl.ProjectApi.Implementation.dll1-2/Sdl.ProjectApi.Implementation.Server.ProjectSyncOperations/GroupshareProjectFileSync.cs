using System;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.Core.Settings;
using Sdl.ProjectApi.Implementation.Interfaces;
using Sdl.ProjectApi.Implementation.Repositories;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Server.ProjectSyncOperations
{
	internal class GroupshareProjectFileSync : IGroupshareSyncOperation
	{
		public void SyncData(IProject project, Sdl.ProjectApi.Implementation.Xml.Project xmlProject, IProjectRepository projectRepository)
		{
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Expected O, but got Unknown
			SettingsBundlesList settingsBundlesList = new SettingsBundlesList(xmlProject.SettingsBundles);
			ProjectRepository projectRepository2 = projectRepository as ProjectRepository;
			foreach (ProjectFile projectFile in xmlProject.ProjectFiles)
			{
				ProjectFile xmlProjectFile = projectRepository2.GetXmlProjectFile(projectFile.Guid);
				if (xmlProjectFile != null)
				{
					foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile in projectFile.LanguageFiles)
					{
						ISettingsBundle languageFileSettings = ((xmlLanguageFile.SettingsBundleGuid != Guid.Empty) ? settingsBundlesList.GetSettingsBundle(xmlLanguageFile.SettingsBundleGuid, null) : null);
						Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = xmlProjectFile.LanguageFiles.FirstOrDefault((Sdl.ProjectApi.Implementation.Xml.LanguageFile lf) => lf.Guid == xmlLanguageFile.Guid);
						if (languageFile != null)
						{
							if (project.IsPublished && xmlLanguageFile.AnalysisStatistics != null && xmlLanguageFile.AnalysisStatistics.Locked == null)
							{
								xmlLanguageFile.AnalysisStatistics.Locked = languageFile.AnalysisStatistics?.Locked;
							}
							if (projectRepository.GetProjectFile(project, languageFile.Guid) is LanguageFile languageFile2)
							{
								ApplySynchronizationDataForFile(xmlLanguageFile, languageFileSettings, languageFile2, project);
							}
						}
						else
						{
							LanguageFile languageFile3 = projectRepository2.AddProjectFile(project, xmlProjectFile.Guid, xmlLanguageFile);
							ApplySynchronizationDataForFile(languageFileSettings, project, languageFile3);
						}
					}
					continue;
				}
				string fileName = projectRepository2.PathUtil.NormalizeFolder(projectFile.Path);
				if (projectRepository.GetProjectFile(project, fileName, projectFile.Name, new Language(projectRepository.SourceLanguage)) is LanguageFile languageFile4)
				{
					languageFile4.RemoveFromProject(deletePhysicalFiles: false);
				}
				projectRepository2.AddProjectFile(projectFile);
				foreach (Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile6 in projectFile.LanguageFiles)
				{
					ISettingsBundle languageFileSettings2 = ((languageFile6.SettingsBundleGuid != Guid.Empty) ? settingsBundlesList.GetSettingsBundle(languageFile6.SettingsBundleGuid, null) : null);
					LanguageFile languageFile5 = projectRepository.GetProjectFile(project, languageFile6.Guid) as LanguageFile;
					ApplySynchronizationDataForFile(languageFileSettings2, project, languageFile5);
				}
			}
		}

		private void ApplySynchronizationDataForFile(Sdl.ProjectApi.Implementation.Xml.LanguageFile xmlLanguageFile, ISettingsBundle languageFileSettings, LanguageFile languageFile, IProject project)
		{
			LanguageFileServerStateSettings settingsGroup = languageFile.Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
			if (languageFileSettings != null)
			{
				LanguageFileServerStateSettings settingsGroup2 = languageFileSettings.GetSettingsGroup<LanguageFileServerStateSettings>();
				settingsGroup.CheckedOutAt.Value = settingsGroup2.CheckedOutAt.Value.ToUniversalTime();
				settingsGroup.CheckedOutTo.Value = Setting<string>.op_Implicit(settingsGroup2.CheckedOutTo);
				settingsGroup.IsCheckedOutOnline.Value = Setting<bool>.op_Implicit(settingsGroup2.IsCheckedOutOnline);
			}
			else
			{
				settingsGroup.CheckedOutAt.Reset();
				settingsGroup.CheckedOutTo.Value = string.Empty;
				settingsGroup.IsCheckedOutOnline.Reset();
			}
			if (!languageFile.CheckedOutToMe)
			{
				languageFile.XmlLanguageFile.FileVersions = xmlLanguageFile.FileVersions;
				languageFile._lazyFileRevisions = null;
				FileVersion fileVersion = languageFile.XmlLanguageFile.FileVersions[languageFile.XmlLanguageFile.FileVersions.Count - 1];
				settingsGroup.LatestServerVersionTimestamp.Value = fileVersion.FileTimeStamp.Ticks;
				settingsGroup.LatestServerVersionNumber.Value = fileVersion.VersionNumber;
				languageFile.XmlLanguageFile.ConfirmationStatistics = xmlLanguageFile.ConfirmationStatistics;
				languageFile.XmlLanguageFile.MergeState = xmlLanguageFile.MergeState;
				languageFile.XmlLanguageFile.MergeStateSpecified = xmlLanguageFile.MergeStateSpecified;
				languageFile.XmlLanguageFile.AnalysisStatistics = xmlLanguageFile.AnalysisStatistics;
			}
			ApplyLanguageFileAssigmentSyncronizationData(languageFileSettings, project, languageFile);
		}

		private void ApplyLanguageFileAssigmentSyncronizationData(ISettingsBundle languageFileSettings, IProject project, LanguageFile languageFile)
		{
			if (languageFileSettings == null || !project.IsPublished)
			{
				return;
			}
			LanguageFileServerPhasesSettings settingsGroup = ((IObjectWithSettings)project).Settings.GetSettingsGroup<LanguageFileServerPhasesSettings>();
			ISettingsBundle settingsBundle = ((IProjectConfiguration)project).SettingsBundlesList.GetSettingsBundle(languageFile.XmlLanguageFile.SettingsBundleGuid, ((IObjectWithSettings)project).Settings);
			if (settingsGroup.Phases.Value == null)
			{
				return;
			}
			foreach (string item in settingsGroup.Phases.Value)
			{
				LanguageFileServerAssignmentsSettings settingsGroup2 = settingsBundle.GetSettingsGroup<LanguageFileServerAssignmentsSettings>("LanguageFileServerAssignmentsSettings_" + item);
				LanguageFileServerAssignmentsSettings settingsGroup3 = languageFileSettings.GetSettingsGroup<LanguageFileServerAssignmentsSettings>("LanguageFileServerAssignmentsSettings_" + item);
				settingsGroup2.AssignedAt = settingsGroup3.AssignedAt;
				settingsGroup2.DueDate = settingsGroup3.DueDate;
				settingsGroup2.AssignedBy = settingsGroup3.AssignedBy;
				settingsGroup2.IsCurrentAssignment = settingsGroup3.IsCurrentAssignment;
				settingsGroup2.Assignees = settingsGroup3.Assignees;
			}
		}

		private void ApplySynchronizationDataForFile(ISettingsBundle languageFileSettings, IProject project, LanguageFile languageFile)
		{
			LanguageFileServerStateSettings settingsGroup = languageFile.Settings.GetSettingsGroup<LanguageFileServerStateSettings>();
			if (languageFileSettings != null)
			{
				LanguageFileServerStateSettings settingsGroup2 = languageFileSettings.GetSettingsGroup<LanguageFileServerStateSettings>();
				settingsGroup.CheckedOutAt.Value = Setting<DateTime>.op_Implicit(settingsGroup2.CheckedOutAt);
				settingsGroup.CheckedOutTo.Value = Setting<string>.op_Implicit(settingsGroup2.CheckedOutTo);
			}
			else
			{
				settingsGroup.CheckedOutAt.Reset();
				settingsGroup.CheckedOutTo.Value = string.Empty;
			}
			FileVersion fileVersion = languageFile.XmlLanguageFile.FileVersions[languageFile.XmlLanguageFile.FileVersions.Count - 1];
			settingsGroup.LatestServerVersionTimestamp.Value = fileVersion.FileTimeStamp.Ticks;
			settingsGroup.LatestServerVersionNumber.Value = fileVersion.VersionNumber;
			ApplyLanguageFileAssigmentSyncronizationData(languageFileSettings, project, languageFile);
		}
	}
}
