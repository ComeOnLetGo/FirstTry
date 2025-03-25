using System.Collections.Generic;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.FileTypeSupport.Framework.NativeApi;

namespace Sdl.ProjectApi.Implementation.SegmentProcessors
{
	public class SegmentMetadataProcessor : ISegmentProcessor
	{
		private const string CreatedByAttribute = "created_by";

		private const string LastModifiedByAttribute = "last_modified_by";

		public IList<string> GetUserIds(ISegment segment)
		{
			List<string> list = new List<string>();
			ITranslationOrigin translationOrigin = segment.Properties.TranslationOrigin;
			if (translationOrigin != null && ((IMetaDataContainer)translationOrigin).HasMetaData)
			{
				foreach (KeyValuePair<string, string> metaDatum in ((IMetaDataContainer)translationOrigin).MetaData)
				{
					if ((!(metaDatum.Key != "created_by") || !(metaDatum.Key != "last_modified_by")) && !list.Contains(metaDatum.Value))
					{
						list.Add(metaDatum.Value);
					}
				}
			}
			return list;
		}
	}
}
