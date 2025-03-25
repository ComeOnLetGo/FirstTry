using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sdl.Core.Bcm.BcmConverters;
using Sdl.Core.Bcm.BcmModel;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.FileTypeSupport.Framework.IntegrationApi;
using Sdl.ProjectApi.Implementation.SegmentProcessors;

namespace Sdl.ProjectApi.Implementation
{
	public class BcmDocumentUsersCollector : IDocumentUsersCollector
	{
		private readonly string _bcmData;

		private readonly IProjectFile _projectFile;

		private readonly ISegmentProcessor _segmentProcessor;

		public BcmDocumentUsersCollector(string bcmData, IProjectFile projectFile, ISegmentProcessor segmentProcessor)
		{
			_bcmData = bcmData;
			_projectFile = projectFile;
			_segmentProcessor = segmentProcessor;
		}

		public IList<string> GetUserIds()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			if (_projectFile == null || _segmentProcessor == null || string.IsNullOrEmpty(_bcmData))
			{
				return null;
			}
			Document val = JsonConvert.DeserializeObject<Document>(_bcmData);
			BcmParser val2 = new BcmParser(val);
			IMultiFileConverter converter = ((IProjectConfiguration)_projectFile.Project).FileTypeConfiguration.FilterManager.GetConverter((IBilingualParser)(object)val2);
			SegmentUserDataCollector segmentUserDataCollector = new SegmentUserDataCollector(_segmentProcessor);
			((IBilingualProcessorContainer)converter).AddBilingualProcessor((IBilingualContentProcessor)(object)segmentUserDataCollector);
			converter.Parse();
			return (from id in segmentUserDataCollector.UserIds()
				where !string.IsNullOrWhiteSpace(id)
				select id).ToList();
		}
	}
}
