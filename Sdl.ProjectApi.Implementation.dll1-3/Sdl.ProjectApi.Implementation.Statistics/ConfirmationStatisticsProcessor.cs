using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.Core.LanguageProcessing.Tokenization;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.FileTypeSupport.Framework.NativeApi;
using Sdl.LanguagePlatform.Core;
using Sdl.LanguagePlatform.Core.Tokenization;
using Sdl.LanguagePlatform.TranslationMemory;
using Sdl.LanguagePlatform.TranslationMemoryApi;
using Sdl.LanguagePlatform.TranslationMemoryTools;
using Sdl.ProjectApi.Settings;

namespace Sdl.ProjectApi.Implementation.Statistics
{
	public class ConfirmationStatisticsProcessor : AbstractBilingualContentHandler
	{
		private const string MergedParagraph = "MergedParagraph";

		private readonly ITranslatableFile _mainTranslatableFile;

		private readonly Tokenizer _tokenizer;

		private readonly ITranslatableFile[] _translatableFiles;

		private readonly ConfirmationStatistics _totalConfirmationStatistics;

		private readonly List<ConfirmationStatistics> _fileConfirmationStatistics;

		private IFileProperties _fileInfo;

		public ConfirmationStatistics[] FileConfirmationStatistics => _fileConfirmationStatistics.ToArray();

		public ConfirmationStatistics TotalConfirmationStatistics => _totalConfirmationStatistics;

		public ITranslatableFile MainTranslatableFile => _mainTranslatableFile;

		public ITranslatableFile[] TranslatableFiles => _translatableFiles;

		private ConfirmationStatistics CurrentFileStatistics => _fileConfirmationStatistics.LastOrDefault();

		private ConfirmationStatisticsProcessor(Tokenizer tokenizer)
		{
			_tokenizer = tokenizer;
			_totalConfirmationStatistics = new ConfirmationStatistics();
			_fileConfirmationStatistics = new List<ConfirmationStatistics>();
		}

		public ConfirmationStatisticsProcessor(ITranslatableFile[] translatableFiles, Tokenizer tokenizer)
			: this(tokenizer)
		{
			_translatableFiles = translatableFiles;
		}

		public ConfirmationStatisticsProcessor(ITranslatableFile mainTranslatableFile, Tokenizer tokenizer)
			: this(tokenizer)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Invalid comparison between Unknown and I4
			_mainTranslatableFile = mainTranslatableFile;
			ITranslatableFile mainTranslatableFile2 = _mainTranslatableFile;
			IMergedTranslatableFile val = (IMergedTranslatableFile)(object)((mainTranslatableFile2 is IMergedTranslatableFile) ? mainTranslatableFile2 : null);
			if (val != null)
			{
				if ((int)val.MergeState != 1)
				{
					throw new ArgumentException("Cannot calculate statistics for a merged file that has not been merged yet.");
				}
				_translatableFiles = val.ChildFiles;
			}
			else
			{
				_translatableFiles = (ITranslatableFile[])(object)new ITranslatableFile[1] { _mainTranslatableFile };
			}
		}

		public override void SetFileProperties(IFileProperties fileInfo)
		{
			_fileInfo = fileInfo;
			if (_fileInfo.IsStartOfFileSection())
			{
				MoveToNextFile();
			}
		}

		public override void ProcessParagraphUnit(IParagraphUnit paragraphUnit)
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Expected O, but got Unknown
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Invalid comparison between Unknown and I4
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			bool flag = paragraphUnit.SegmentPairs.Count() == 1;
			foreach (ISegmentPair segmentPair in paragraphUnit.SegmentPairs)
			{
				if (IsMergedParagraph(segmentPair.Target))
				{
					continue;
				}
				ConfirmationStatistics currentFileStatistics = CurrentFileStatistics;
				ICountData val = ((currentFileStatistics != null) ? ((AbstractConfirmationStatistics)currentFileStatistics)[segmentPair.Properties.ConfirmationLevel] : null);
				if (val != null)
				{
					SourceContentSettings settingsGroup = ((IObjectWithSettings)_translatableFiles[0]).Settings.GetSettingsGroup<SourceContentSettings>();
					Language val2 = (((ILanguageFile)_translatableFiles[0]).IsSource ? ((IProjectFile)_translatableFiles[0]).Language : ((ILanguageFile)_translatableFiles[0]).LanguageDirection.SourceLanguage);
					Segment val3 = TUConverter.BuildLinguaSegment(CultureCode.op_Implicit(val2.CultureInfo), segmentPair.Source, false, true, false);
					List<Token> tokens = _tokenizer.GetTokens(val3, settingsGroup.EnableIcuTokenization);
					WordCountFlags wordCountFlags = GetWordCountFlags();
					WordCounts val4 = new WordCounts((IList<Token>)tokens, ((Enum)wordCountFlags).HasFlag((Enum)(object)(WordCountFlags)1), ((Enum)wordCountFlags).HasFlag((Enum)(object)(WordCountFlags)2), ((Enum)wordCountFlags).HasFlag((Enum)(object)(WordCountFlags)4), ((Enum)wordCountFlags).HasFlag((Enum)(object)(WordCountFlags)8));
					if (flag && paragraphUnit.Properties.SourceCount != null)
					{
						SourceCount sourceCount = paragraphUnit.Properties.SourceCount;
						val.Increment(((int)sourceCount.Unit == 1) ? Convert.ToInt32(sourceCount.Value) : val4.Characters, ((int)sourceCount.Unit == 0) ? Convert.ToInt32(sourceCount.Value) : val4.Words, 1, 0, 0);
					}
					else
					{
						val.Increment(val4.Characters, val4.Words, val4.Segments, val4.Placeables, val4.Tags);
					}
				}
			}
		}

		private WordCountFlags GetWordCountFlags()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			WordCountFlags result = (WordCountFlags)4;
			if (((ILanguageFile)_translatableFiles[0]).LanguageDirection == null)
			{
				return result;
			}
			ProjectCascade projectCascade = ((ILanguageFile)_translatableFiles[0]).LanguageDirection.CreateCascade(refreshCache: false);
			foreach (ProjectCascadeEntry cascadeEntry in ((Cascade<ProjectCascadeEntry>)projectCascade).CascadeEntries)
			{
				ITranslationProviderLanguageDirection translationProviderLanguageDirection = ((CascadeEntry)cascadeEntry).TranslationProviderLanguageDirection;
				ITranslationMemoryLanguageDirection val = (ITranslationMemoryLanguageDirection)(object)((translationProviderLanguageDirection is ITranslationMemoryLanguageDirection) ? translationProviderLanguageDirection : null);
				if (val != null)
				{
					ITranslationMemory translationProvider = val.TranslationProvider;
					ITranslationMemory2015 val2 = (ITranslationMemory2015)(object)((translationProvider is ITranslationMemory2015) ? translationProvider : null);
					if (val2 != null)
					{
						result = val2.WordCountFlags;
						break;
					}
				}
			}
			return result;
		}

		public override void FileComplete()
		{
			if (_fileInfo.IsEndOfFileSection())
			{
				UpdateCurrentFileStatistics();
				UpdateTotalConfirmationStatistics();
			}
		}

		public override void Complete()
		{
			((AbstractBilingualContentHandler)this).Complete();
			((AbstractConfirmationStatistics)_totalConfirmationStatistics).Update();
			((AbstractBilingualFileTypeComponent)this).MessageReporter = null;
			((AbstractBilingualFileTypeComponent)this).ItemFactory = null;
		}

		private void MoveToNextFile()
		{
			_fileConfirmationStatistics.Add(new ConfirmationStatistics());
		}

		private void UpdateTotalConfirmationStatistics()
		{
			if (CurrentFileStatistics != null)
			{
				((AbstractConfirmationStatistics)_totalConfirmationStatistics).Increment((IConfirmationStatistics)(object)CurrentFileStatistics);
			}
		}

		private void UpdateCurrentFileStatistics()
		{
			ConfirmationStatistics currentFileStatistics = CurrentFileStatistics;
			if (currentFileStatistics != null)
			{
				((AbstractConfirmationStatistics)currentFileStatistics).Update();
			}
		}

		private bool IsMergedParagraph(ISegment segment)
		{
			ITranslationOrigin translationOrigin = segment.Properties.TranslationOrigin;
			return ((translationOrigin != null) ? translationOrigin.OriginSystem : null) == "MergedParagraph";
		}
	}
}
