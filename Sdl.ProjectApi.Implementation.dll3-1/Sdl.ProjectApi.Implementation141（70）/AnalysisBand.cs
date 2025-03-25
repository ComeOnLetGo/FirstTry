namespace Sdl.ProjectApi.Implementation
{
	internal class AnalysisBand : IAnalysisBand
	{
		private readonly int _minimumMatchValue;

		private readonly int _maximumMatchValue;

		public int MaximumMatchValue => _maximumMatchValue;

		public int MinimumMatchValue => _minimumMatchValue;

		public AnalysisBand(int minimumMatchValue, int maximumMatchValue)
		{
			_minimumMatchValue = minimumMatchValue;
			_maximumMatchValue = maximumMatchValue;
		}

		public override string ToString()
		{
			return $"{MinimumMatchValue}%-{MaximumMatchValue}%";
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			IAnalysisBand val = (IAnalysisBand)((obj is IAnalysisBand) ? obj : null);
			if (val == null)
			{
				return false;
			}
			if (val.MinimumMatchValue == MinimumMatchValue)
			{
				return val.MaximumMatchValue == MaximumMatchValue;
			}
			return false;
		}
	}
}
