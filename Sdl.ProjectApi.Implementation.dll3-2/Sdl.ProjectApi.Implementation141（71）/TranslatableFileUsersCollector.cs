using System;
using System.Collections.Generic;
using System.Linq;
using Sdl.Core.Globalization;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectApi.Implementation.SegmentProcessors;

namespace Sdl.ProjectApi.Implementation
{
	public class TranslatableFileUsersCollector : IDocumentUsersCollector
	{
		private readonly ITranslatableFile[] _translatableFiles;

		private readonly ISegmentProcessor _segmentProcessor;

		public TranslatableFileUsersCollector(ITranslatableFile[] translatableFiles, ISegmentProcessor segmentProcessor)
		{
			_translatableFiles = translatableFiles;
			_segmentProcessor = segmentProcessor;
		}

		public IList<string> GetUserIds()
		{
			HashSet<string> hashSet = new HashSet<string>();
			ITranslatableFile[] translatableFiles = _translatableFiles;
			foreach (ITranslatableFile translatableFile in translatableFiles)
			{
				hashSet.UnionWith(GetUserIdsFromFile(translatableFile));
			}
			return hashSet.Where((string id) => !string.IsNullOrWhiteSpace(id)).ToList();
		}

		private HashSet<string> GetUserIdsFromFile(ITranslatableFile translatableFile)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Expected O, but got Unknown
			string[] array = new string[1] { ((IProjectFile)translatableFile).LocalFilePath };
			Language val = (((ILanguageFile)translatableFile).IsSource ? ((IProjectFile)translatableFile).Language : ((IProjectFile)translatableFile).Project.SourceLanguage);
			((IProjectConfiguration)((IProjectFile)translatableFile).Project).FileTypeConfiguration.FilterManager.SettingsBundle = ((IObjectWithSettings)((IProjectFile)translatableFile).Project).Settings;
			BilingualDocument val2 = new BilingualDocument();
			IMultiFileConverter converterToBilingual = ((IProjectConfiguration)((IProjectFile)translatableFile).Project).FileTypeConfiguration.FilterManager.GetConverterToBilingual(array, val2.ContentInput, (BilingualDocumentOutputPropertiesProvider)null, val.CultureInfo, (Codepage)null, (EventHandler<MessageEventArgs>)null);
			SegmentUserDataCollector segmentUserDataCollector = new SegmentUserDataCollector(_segmentProcessor);
			((IBilingualProcessorContainer)converterToBilingual).AddBilingualProcessor((IBilingualContentProcessor)(object)segmentUserDataCollector);
			converterToBilingual.Parse();
			return segmentUserDataCollector.UserIds();
		}
	}
}
