using System.Collections.Generic;
using System.Linq;
using Sdl.FileTypeSupport.Framework.BilingualApi;

namespace Sdl.ProjectApi.Implementation.SegmentProcessors
{
	public abstract class SegmentProcessor : ISegmentProcessor
	{
		public List<ISegmentProcessor> Processors { get; } = new List<ISegmentProcessor>();


		public IList<string> GetUserIds(ISegment segment)
		{
			List<string> list = new List<string>();
			foreach (ISegmentProcessor processor in Processors)
			{
				list.AddRange(processor.GetUserIds(segment));
			}
			return list.Distinct().ToList();
		}
	}
}
