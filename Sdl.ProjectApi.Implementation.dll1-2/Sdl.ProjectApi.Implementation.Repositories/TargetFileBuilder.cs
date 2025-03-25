using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.ApiClientSdk.StudioBFF.Models;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.LanguageCloud.Builders;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Repositories
{
	public class TargetFileBuilder : FileBuilderBase
	{
		private void AddStatisticsToFile(LightFile file, Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile)
		{
			if (file.AnalysisStatistics != null)
			{
				languageFile.AnalysisStatistics = GetAnalysisStatistics(file.AnalysisStatistics);
			}
			if (file.ConfirmationStatistics != null)
			{
				languageFile.ConfirmationStatistics = GetConfirmationStatistics(file.ConfirmationStatistics);
			}
		}

		public void UpdateTargetFile(LightFile file, Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile)
		{
			AddStatisticsToFile(file, languageFile);
			languageFile.FileVersions.FirstOrDefault().Guid = Guid.Parse(file.LatestFileVersion);
		}

		public Sdl.ProjectApi.Implementation.Xml.LanguageFile CreateTargetLanguageFile(LightFile file, Language targetLanguage)
		{
			string filePath = (file.Name.EndsWith(".sdlxliff") ? (((LanguageBase)targetLanguage).IsoAbbreviation + "\\" + file.Name) : (((LanguageBase)targetLanguage).IsoAbbreviation + "\\" + file.Name + ".sdlxliff"));
			FileVersion latestXmlFileVersion = CreateXmlLanguageFileVersionForLC(Guid.Parse(file.LatestFileVersion), file.Name.EndsWith(".sdlxliff") ? file.Name : (file.Name + ".sdlxliff"), filePath, 1);
			Sdl.ProjectApi.Implementation.Xml.LanguageFile languageFile = CreateXmlLanguageFileForLC(targetLanguage, Guid.Parse(file.Id), latestXmlFileVersion);
			AddStatisticsToFile(file, languageFile);
			return languageFile;
		}

		private AnalysisStatistics GetAnalysisStatistics(AnalysisStatistics analysisStatistics)
		{
			AnalysisStatistics parsedAnalysisStatistics = new AnalysisStatistics
			{
				WordCountStatus = ValueStatus.Complete,
				AnalysisFileTimeStampSpecified = false,
				AnalysisStatus = ValueStatus.Complete,
				Exact = analysisStatistics.ExactMatch.GetCountData(),
				InContextExact = analysisStatistics.InContextExactMatch.GetCountData(),
				New = analysisStatistics.New.GetCountData(),
				Total = analysisStatistics.Total.GetCountData(),
				Repetitions = analysisStatistics.Repetitions.GetCountData(),
				Perfect = analysisStatistics.PerfectMatch.GetCountData()
			};
			if (analysisStatistics.Locked != null)
			{
				parsedAnalysisStatistics.Locked = analysisStatistics.Locked.GetCountData();
			}
			parsedAnalysisStatistics.Fuzzy = new List<CountData>();
			analysisStatistics.Fuzzy.ToList().ForEach(delegate(AnalysisFuzzy fuzzyStatistic)
			{
				parsedAnalysisStatistics.Fuzzy.Add(fuzzyStatistic.CountData.GetCountData());
			});
			return parsedAnalysisStatistics;
		}

		private ConfirmationStatistics GetConfirmationStatistics(ConfirmationStatistics confirmationStatistics)
		{
			return new ConfirmationStatistics
			{
				ApprovedSignOff = confirmationStatistics.ApprovedSignOff.GetCountData(),
				ApprovedTranslation = confirmationStatistics.ApprovedTranslation.GetCountData(),
				Draft = confirmationStatistics.Draft.GetCountData(),
				FileTimeStampSpecified = false,
				RejectedSignOff = confirmationStatistics.RejectedSignOff.GetCountData(),
				RejectedTranslation = confirmationStatistics.RejectedTranslation.GetCountData(),
				Status = ValueStatus.Complete,
				Translated = confirmationStatistics.Translated.GetCountData(),
				Unspecified = confirmationStatistics.NotTranslated.GetCountData()
			};
		}
	}
}
