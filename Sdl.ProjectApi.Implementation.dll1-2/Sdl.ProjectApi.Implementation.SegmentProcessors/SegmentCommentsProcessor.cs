using System.Collections.Generic;
using System.Linq;
using Sdl.FileTypeSupport.Framework.BilingualApi;
using Sdl.FileTypeSupport.Framework.NativeApi;

namespace Sdl.ProjectApi.Implementation.SegmentProcessors
{
	public class SegmentCommentsProcessor : ISegmentProcessor
	{
		private List<ICommentMarker> GetComments(ISegment segment)
		{
			List<ICommentMarker> list = new List<ICommentMarker>();
			CollectCommentItems((IAbstractMarkupDataContainer)(object)segment, list);
			return list;
		}

		private void CollectCommentItems(IAbstractMarkupDataContainer container, List<ICommentMarker> comments)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			foreach (IAbstractMarkupData item in (IEnumerable<IAbstractMarkupData>)container)
			{
				ICommentMarker val = (ICommentMarker)(object)((item is ICommentMarker) ? item : null);
				if (val != null)
				{
					comments.Add(val);
				}
				else if (item is IAbstractMarkupDataContainer)
				{
					IAbstractMarkupDataContainer container2 = (IAbstractMarkupDataContainer)item;
					CollectCommentItems(container2, comments);
				}
			}
		}

		public IList<string> GetUserIds(ISegment segment)
		{
			List<string> list = new List<string>();
			List<ICommentMarker> comments = GetComments(segment);
			foreach (ICommentMarker item in comments)
			{
				if (item.Comments.Comments.Any())
				{
					list.AddRange(item.Comments.Comments.Select((IComment x) => x.Author).Distinct());
				}
			}
			return list.Distinct().ToList();
		}
	}
}
