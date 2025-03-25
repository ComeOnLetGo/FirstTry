using System.Collections.Generic;
using System.Linq;
using Sdl.FileTypeSupport.Framework.BilingualApi;

namespace Sdl.ProjectApi.Implementation.SegmentProcessors
{
	public class SegmentRevisionsProcessor : ISegmentProcessor
	{
		private List<IRevisionMarker> GetRevisions(ISegment segment)
		{
			List<IRevisionMarker> list = new List<IRevisionMarker>();
			CollectRevisionItems((IAbstractMarkupDataContainer)(object)segment, list);
			return list;
		}

		private void CollectRevisionItems(IAbstractMarkupDataContainer container, List<IRevisionMarker> revisions)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			foreach (IAbstractMarkupData item in (IEnumerable<IAbstractMarkupData>)container)
			{
				IRevisionMarker val = (IRevisionMarker)(object)((item is IRevisionMarker) ? item : null);
				if (val != null)
				{
					revisions.Add(val);
				}
				else if (item is IAbstractMarkupDataContainer)
				{
					IAbstractMarkupDataContainer container2 = (IAbstractMarkupDataContainer)item;
					CollectRevisionItems(container2, revisions);
				}
			}
		}

		public IList<string> GetUserIds(ISegment segment)
		{
			List<string> list = new List<string>();
			List<IRevisionMarker> revisions = GetRevisions(segment);
			list.AddRange(revisions.Select((IRevisionMarker x) => x.Properties.Author).Distinct());
			return list;
		}
	}
}
