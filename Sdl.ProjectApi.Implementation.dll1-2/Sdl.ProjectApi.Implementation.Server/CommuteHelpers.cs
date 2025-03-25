using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Server;
using Sdl.ProjectApi.Server.Model;

namespace Sdl.ProjectApi.Implementation.Server
{
	public static class CommuteHelpers
	{
		public static ConfirmationStatistics ToCommuteConfirmationStatistics(this IConfirmationStatistics confirmationStatistics)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Expected O, but got Unknown
			if ((int)confirmationStatistics.Status == 0)
			{
				return null;
			}
			return new ConfirmationStatistics
			{
				Unspecified = confirmationStatistics[(ConfirmationLevel)0].ToCommuteCountData(),
				Draft = confirmationStatistics[(ConfirmationLevel)1].ToCommuteCountData(),
				Translated = confirmationStatistics[(ConfirmationLevel)2].ToCommuteCountData(),
				RejectedTranslation = confirmationStatistics[(ConfirmationLevel)3].ToCommuteCountData(),
				ApprovedTranslation = confirmationStatistics[(ConfirmationLevel)4].ToCommuteCountData(),
				RejectedSignOff = confirmationStatistics[(ConfirmationLevel)5].ToCommuteCountData(),
				ApprovedSignOff = confirmationStatistics[(ConfirmationLevel)6].ToCommuteCountData()
			};
		}

		public static CountData ToCommuteCountData(this ICountData countData)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			return new CountData
			{
				Characters = countData.Characters,
				Placeables = countData.Placeables,
				Segments = countData.Segments,
				Tags = countData.Tags,
				Words = countData.Words
			};
		}

		public static ProjectFileRole ToCommuteProjectFileRole(this FileRole fileRole)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Expected I4, but got Unknown
			return (ProjectFileRole)((int)fileRole switch
			{
				0 => 0, 
				1 => 1, 
				2 => 3, 
				3 => 2, 
				4 => 4, 
				_ => throw new ArgumentOutOfRangeException("fileRole"), 
			});
		}

		public static AnalysisStatistics ToCommuteAnalysisStatistics(this IAnalysisStatistics analysisStatistics)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Invalid comparison between Unknown and I4
			AnalysisStatistics val = new AnalysisStatistics();
			if ((int)((IWordCountStatistics)analysisStatistics).WordCountStatus == 3)
			{
				val.Total = ((IWordCountStatistics)analysisStatistics).Total.ToCommuteCountData();
				val.UpdateExistingWordCounts = true;
			}
			if ((int)analysisStatistics.AnalysisStatus == 3)
			{
				val.Exact = analysisStatistics.Exact.ToCommuteCountData();
				val.InContextExact = analysisStatistics.InContextExact.ToCommuteCountData();
				val.New = analysisStatistics.New.ToCommuteCountData();
				val.Perfect = analysisStatistics.Perfect.ToCommuteCountData();
				val.Repeated = ((IWordCountStatistics)analysisStatistics).Repetitions.ToCommuteCountData();
				val.Fuzzy = analysisStatistics.Fuzzy.Select((IFuzzyCountData f) => ((ICountData)(object)f).ToCommuteCountData()).ToArray();
				val.UpdateExistingAnalysisStatistics = false;
			}
			return val;
		}

		public static DataTransferEventArgs ToDataTransferEventArgs(this FileTransferEventArgs e)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			//IL_004e: Expected O, but got Unknown
			DataTransferEventArgs val = new DataTransferEventArgs
			{
				BytesTransferred = e.BytesTransferred,
				TotalBytes = e.TotalBytes,
				FileBytesTransferred = e.FileBytesTransferred,
				Filename = e.Filename,
				FileTotalBytes = e.FileTotalBytes
			};
			((CancelEventArgs)val).Cancel = ((CancelEventArgs)(object)e).Cancel;
			return val;
		}

		public static FileVersionInfo GetLastFileVersion(this LanguageFileInfo languageFileInfo)
		{
			if (languageFileInfo.NewFileVersions.Length == 0)
			{
				return null;
			}
			return languageFileInfo.NewFileVersions[languageFileInfo.NewFileVersions.Length - 1];
		}

		public static Dictionary<string, FileVersionInfo> GetLastFileVersionPerFileName(this LanguageFileInfo languageFileInfo)
		{
			Dictionary<string, FileVersionInfo> dictionary = new Dictionary<string, FileVersionInfo>();
			IEnumerable<string> enumerable = languageFileInfo.NewFileVersions.Select((FileVersionInfo fv) => fv.FileName).Distinct();
			foreach (string fileName in enumerable)
			{
				int num = Array.FindLastIndex(languageFileInfo.NewFileVersions, (FileVersionInfo fv) => fv.FileName == fileName);
				dictionary[fileName] = languageFileInfo.NewFileVersions[num];
			}
			return dictionary;
		}

		public static MergeState ToCommuteMergeState(this MergeState mergeState)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected I4, but got Unknown
			return (MergeState)((int)mergeState switch
			{
				0 => 0, 
				1 => 1, 
				2 => 2, 
				_ => throw new ArgumentOutOfRangeException("mergeState"), 
			});
		}

		public static ProjectStatus ToProjectStatus(this ProjectStatus projectStatus)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected I4, but got Unknown
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			switch (projectStatus - 1)
			{
			default:
				if ((int)projectStatus != 8)
				{
					break;
				}
				return (ProjectStatus)4;
			case 0:
				return (ProjectStatus)1;
			case 1:
				return (ProjectStatus)2;
			case 3:
				return (ProjectStatus)3;
			case 2:
				break;
			}
			throw new ArgumentOutOfRangeException("projectStatus");
		}
	}
}
