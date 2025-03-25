using System.Collections.Generic;
using Sdl.FileTypeSupport.Framework.BilingualApi;

namespace Sdl.ProjectApi.Implementation.SegmentProcessors
{
	public class SegmentUserDataCollector : AbstractBilingualContentProcessor, ISegmentDataCollector
	{
		private readonly List<string> _userIds = new List<string>();

		private readonly ISegmentProcessor _segmentProcessor;

		public SegmentUserDataCollector(ISegmentProcessor segmentProcessor)
		{
			_segmentProcessor = segmentProcessor;
		}

		public override void ProcessParagraphUnit(IParagraphUnit paragraphUnit)
		{
			foreach (ISegmentPair segmentPair in paragraphUnit.SegmentPairs)
			{
				ISegment target = segmentPair.Target;
				IList<string> userIds = _segmentProcessor.GetUserIds(target);
				foreach (string item in userIds)
				{
					if (!_userIds.Contains(item))
					{
						_userIds.Add(item);
					}
				}
			}
			((AbstractBilingualContentProcessor)this).ProcessParagraphUnit(paragraphUnit);
		}

		public HashSet<string> UserIds()
		{
			return new HashSet<string>(_userIds);
		}
	}
}
