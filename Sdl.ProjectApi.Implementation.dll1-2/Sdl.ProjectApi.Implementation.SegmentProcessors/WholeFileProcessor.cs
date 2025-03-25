namespace Sdl.ProjectApi.Implementation.SegmentProcessors
{
	public class WholeFileProcessor : SegmentProcessor
	{
		public WholeFileProcessor()
		{
			base.Processors.Add((ISegmentProcessor)(object)new SegmentCommentsProcessor());
			base.Processors.Add((ISegmentProcessor)(object)new SegmentRevisionsProcessor());
			base.Processors.Add((ISegmentProcessor)(object)new SegmentMetadataProcessor());
		}
	}
}
