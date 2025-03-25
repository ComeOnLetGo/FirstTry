using System.Collections.Generic;

namespace Sdl.ProjectApi.Implementation.Statistics
{
	public class CompoundAnalysisStatistics : IAnalysisStatistics, IWordCountStatistics
	{
		private class AnalysisBandComparer : IComparer<IAnalysisBand>
		{
			public int Compare(IAnalysisBand x, IAnalysisBand y)
			{
				return x.MinimumMatchValue.CompareTo(y.MinimumMatchValue);
			}
		}

		private readonly CountData _repetitions;

		private readonly CountData _exact;

		private readonly CountData _inContextExact;

		private readonly CountData _new;

		private readonly CountData _perfect;

		private readonly CountData _total;

		private readonly CountData _locked;

		public ICountData Repetitions => (ICountData)(object)_repetitions;

		public ICountData Perfect => (ICountData)(object)_perfect;

		public ICountData Locked => (ICountData)(object)_locked;

		public ICountData Exact => (ICountData)(object)_exact;

		public ICountData InContextExact => (ICountData)(object)_inContextExact;

		public ICountData New => (ICountData)(object)_new;

		public IFuzzyCountData[] Fuzzy { get; }

		public ICountData Total => (ICountData)(object)_total;

		public ValueStatus AnalysisStatus { get; }

		public bool CanUpdate => false;

		public ValueStatus WordCountStatus { get; }

		public CompoundAnalysisStatistics(IEnumerable<IAnalysisStatistics> stats)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_018b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			_repetitions = new CountData();
			_exact = new CountData();
			_inContextExact = new CountData();
			_new = new CountData();
			_perfect = new CountData();
			_total = new CountData();
			_locked = new CountData();
			SortedDictionary<IAnalysisBand, IFuzzyCountData> sortedDictionary = new SortedDictionary<IAnalysisBand, IFuzzyCountData>(new AnalysisBandComparer());
			AnalysisStatus = (ValueStatus)3;
			WordCountStatus = (ValueStatus)3;
			bool flag = false;
			bool flag2 = false;
			foreach (IAnalysisStatistics stat in stats)
			{
				_repetitions.Increment(((IWordCountStatistics)stat).Repetitions);
				_exact.Increment(stat.Exact);
				_inContextExact.Increment(stat.InContextExact);
				_new.Increment(stat.New);
				_perfect.Increment(stat.Perfect);
				_locked.Increment(stat.Locked);
				_total.Increment(((IWordCountStatistics)stat).Total);
				IFuzzyCountData[] fuzzy = stat.Fuzzy;
				foreach (IFuzzyCountData val in fuzzy)
				{
					IAnalysisBand band = val.Band;
					if (!sortedDictionary.TryGetValue(band, out var value))
					{
						value = (sortedDictionary[band] = (IFuzzyCountData)(object)new FuzzyCountData(band, null));
					}
					((ICountData)value).Increment((ICountData)(object)val);
				}
				ValueStatus wordCountStatus = ((IWordCountStatistics)stat).WordCountStatus;
				WordCountStatus = WordCountStatus.CombineValueStatus(wordCountStatus);
				if ((int)wordCountStatus != 0)
				{
					flag = true;
				}
				ValueStatus analysisStatus = stat.AnalysisStatus;
				AnalysisStatus = AnalysisStatus.CombineValueStatus(analysisStatus);
				if ((int)analysisStatus != 0)
				{
					flag2 = true;
				}
			}
			if (!flag)
			{
				WordCountStatus = (ValueStatus)0;
			}
			if (!flag2)
			{
				AnalysisStatus = (ValueStatus)0;
			}
			IFuzzyCountData[] array = (IFuzzyCountData[])(object)new FuzzyCountData[sortedDictionary.Values.Count];
			Fuzzy = array;
			sortedDictionary.Values.CopyTo(Fuzzy, 0);
		}

		public void UpdateAll()
		{
		}

		public void UpdateWordCountOnly()
		{
		}

		public void Reset()
		{
		}
	}
}
