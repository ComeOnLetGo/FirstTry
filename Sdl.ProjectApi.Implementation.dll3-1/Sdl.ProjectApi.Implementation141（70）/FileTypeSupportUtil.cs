using System;
using System.IO;
using Sdl.Core.Globalization;
using Sdl.Desktop.Platform.Interfaces.SystemIO;
using Sdl.FileTypeSupport.Framework;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.FileTypeSupport.Framework.NativeApi;

namespace Sdl.ProjectApi.Implementation
{
	public static class FileTypeSupportUtil
	{
		public static void ReportMessage(IExecutingTaskFile file, MessageEventArgs args)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			((IFileExecutionMessageReporter)file).ReportMessage(args.Origin, args.Message, GetMessageLevel(args.Level), args.FromLocation, args.UptoLocation);
		}

		private static MessageLevel GetMessageLevel(ErrorLevel level)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Expected I4, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			return (MessageLevel)((level - 1) switch
			{
				2 => 2, 
				1 => 1, 
				0 => 0, 
				_ => throw new ArgumentException($"Unexpected error level: {level}.", "level"), 
			});
		}

		public static void ResolveLanguages(IMultiFileConverter converter, Language projectSourceLanguage, Language projectTargetLanguage)
		{
			if (converter == null)
			{
				throw new ArgumentNullException("converter");
			}
			if (UseProjectLanguage(converter.DocumentInfo.SourceLanguage, projectSourceLanguage, converter.DetectedSourceLanguage))
			{
				converter.DocumentInfo.SourceLanguage = projectSourceLanguage;
				converter.ApplyDocumentPropertiesToExtractors();
			}
			if (UseProjectLanguage(converter.DocumentInfo.TargetLanguage, projectTargetLanguage, converter.DetectedTargetLanguage))
			{
				converter.DocumentInfo.TargetLanguage = projectTargetLanguage;
				converter.ApplyDocumentPropertiesToExtractors();
			}
		}

		private static bool UseProjectLanguage(Language converterLanguage, Language projectLanguage, Pair<Language, DetectionLevel> detectedLanguage)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			if (!LanguageBase.IsNullOrInvalid((LanguageBase)(object)projectLanguage))
			{
				if (LanguageBase.IsNullOrInvalid((LanguageBase)(object)converterLanguage))
				{
					return true;
				}
				if (detectedLanguage != null && (int)detectedLanguage.Second != 3)
				{
					return true;
				}
			}
			return false;
		}

		public static string FindDependencyFile(IExecutingTaskFile executingTaskFile, IDependencyFileProperties fileInfo, IFile fileWrapper)
		{
			string result = null;
			if (executingTaskFile != null && executingTaskFile.TranslatableFile != null && ((ILanguageFile)executingTaskFile.TranslatableFile).FolderInProject != null)
			{
				string text = ((ILanguageFile)executingTaskFile.TranslatableFile).FolderInProject;
				if (text.Length > 0 && text[text.Length - 1] == Path.DirectorySeparatorChar)
				{
					text = Path.GetDirectoryName(text);
				}
				string directoryName = Path.GetDirectoryName(((IProjectFile)executingTaskFile.TranslatableFile).LocalFilePath);
				while (!string.IsNullOrEmpty(directoryName) && !string.IsNullOrEmpty(text) && Path.GetFileName(directoryName) == Path.GetFileName(text))
				{
					directoryName = Path.GetDirectoryName(directoryName);
					text = Path.GetDirectoryName(text);
				}
				if (!string.IsNullOrEmpty(directoryName))
				{
					bool flag = false;
					if (((IProjectFile)executingTaskFile.TranslatableFile).Project != null && ((IProjectFile)executingTaskFile.TranslatableFile).Project.IsInPlace)
					{
						flag = true;
					}
					if (flag || string.Equals(Path.GetFileName(directoryName), ((IProjectFile)executingTaskFile.TranslatableFile).Language.CultureInfo.Name, StringComparison.CurrentCultureIgnoreCase))
					{
						string path = directoryName;
						if (!flag)
						{
							path = Path.Combine(Path.GetDirectoryName(directoryName), ((IProjectFile)executingTaskFile.TranslatableFile).Project.SourceLanguage.CultureInfo.Name);
						}
						path = Path.Combine(path, ((ILanguageFile)executingTaskFile.TranslatableFile).FolderInProject);
						path = Path.Combine(path, fileInfo.PathRelativeToConverted);
						if (fileWrapper.Exists(path))
						{
							return path;
						}
					}
				}
			}
			return result;
		}
	}
}
