using System;
using Sdl.ProjectApi.Implementation.Statistics;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation
{
	public class WordCountStatistics : IWordCountStatistics
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.WordCountStatistics _xmlWordCountStatistics;

		public ICountData Repetitions { get; }

		public ICountData Total { get; }

		public ValueStatus WordCountStatus => EnumConvert.ConvertValueStatus(_xmlWordCountStatistics.WordCountStatus);

		public WordCountStatistics(ITranslatableFile[] files)
		{
			_xmlWordCountStatistics = ComputeWordCountStatistics(files);
			EnsureCountDataObjects();
			Repetitions = (ICountData)(object)new CountDataRepository(_xmlWordCountStatistics.Repetitions);
			Total = (ICountData)(object)new CountDataRepository(_xmlWordCountStatistics.Total);
		}

		private void EnsureCountDataObjects()
		{
			if (_xmlWordCountStatistics.Repetitions == null)
			{
				_xmlWordCountStatistics.WordCountStatus = ValueStatus.None;
				_xmlWordCountStatistics.Repetitions = new Sdl.ProjectApi.Implementation.Xml.CountData();
				_xmlWordCountStatistics.Total = new Sdl.ProjectApi.Implementation.Xml.CountData();
			}
		}

		public void Reset()
		{
			EnsureCountDataObjects();
			Repetitions.Reset();
			Total.Reset();
			_xmlWordCountStatistics.WordCountStatus = ValueStatus.None;
		}

		private Sdl.ProjectApi.Implementation.Xml.WordCountStatistics ComputeWordCountStatistics(ITranslatableFile[] files)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Invalid comparison between Unknown and I4
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			Sdl.ProjectApi.Implementation.Xml.WordCountStatistics wordCountStatistics = new Sdl.ProjectApi.Implementation.Xml.WordCountStatistics
			{
				Repetitions = new Sdl.ProjectApi.Implementation.Xml.CountData(),
				Total = new Sdl.ProjectApi.Implementation.Xml.CountData()
			};
			ValueStatus val = (ValueStatus)3;
			bool flag = false;
			foreach (ITranslatableFile val2 in files)
			{
				IAnalysisStatistics analysisStatistics = val2.AnalysisStatistics;
				val = val.CombineValueStatus(((IWordCountStatistics)analysisStatistics).WordCountStatus);
				ValueStatus wordCountStatus = ((IWordCountStatistics)analysisStatistics).WordCountStatus;
				if ((int)wordCountStatus != 0)
				{
					if (wordCountStatus - 2 > 1)
					{
						ValueStatus wordCountStatus2 = ((IWordCountStatistics)analysisStatistics).WordCountStatus;
						throw new ArgumentException("Unexpected ValueStatus for file statistics: " + ((object)(ValueStatus)(ref wordCountStatus2)).ToString());
					}
					Increment(wordCountStatistics.Total, ((IWordCountStatistics)analysisStatistics).Total);
					Increment(wordCountStatistics.Repetitions, ((IWordCountStatistics)analysisStatistics).Repetitions);
					flag = true;
				}
			}
			if (!flag)
			{
				val = (ValueStatus)0;
			}
			wordCountStatistics.WordCountStatus = EnumConvert.ConvertValueStatus(val);
			return wordCountStatistics;
		}

		private void Increment(Sdl.ProjectApi.Implementation.Xml.CountData data, ICountData update)
		{
			data.Characters += update.Characters;
			data.Words += update.Words;
			data.Segments += update.Segments;
		}
	}
}
