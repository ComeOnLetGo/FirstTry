using System;
using System.IO;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Statistics
{
	public class AnalysisStatistics : AbstractProjectItem, IAnalysisStatistics, IWordCountStatistics
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.AnalysisStatistics _xmlAnalysisStatistics;

		private readonly IFuzzyCountData[] _fuzzyCountData;

		private readonly ICountData _repetitions;

		private readonly ICountData _exact;

		private readonly ICountData _inContextExact;

		private readonly ICountData _new;

		private readonly ICountData _perfect;

		private readonly ICountData _total;

		private readonly ICountData _locked;

		private readonly bool _canUpdate;

		private readonly ILanguageDirection _languageDirection;

		private readonly TranslatableFile _translatableFile;

		public ICountData Repetitions => _repetitions;

		public ICountData Perfect => _perfect;

		public ICountData Locked => _locked;

		public ICountData Exact => _exact;

		public ICountData InContextExact => _inContextExact;

		public ICountData New => _new;

		public IFuzzyCountData[] Fuzzy => _fuzzyCountData;

		public ICountData Total => _total;

		public ValueStatus WordCountStatus
		{
			get
			{
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				if (_translatableFile != null && _xmlAnalysisStatistics.WordCountFileTimeStampSpecified && (_xmlAnalysisStatistics.WordCountStatus == ValueStatus.Complete || _xmlAnalysisStatistics.WordCountStatus == ValueStatus.OutOfDate))
				{
					return (ValueStatus)3;
				}
				return EnumConvert.ConvertValueStatus(_xmlAnalysisStatistics.WordCountStatus);
			}
		}

		public ValueStatus AnalysisStatus
		{
			get
			{
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				if (_translatableFile != null && _xmlAnalysisStatistics.AnalysisFileTimeStampSpecified && (_xmlAnalysisStatistics.AnalysisStatus == ValueStatus.Complete || _xmlAnalysisStatistics.AnalysisStatus == ValueStatus.OutOfDate))
				{
					return (ValueStatus)3;
				}
				return EnumConvert.ConvertValueStatus(_xmlAnalysisStatistics.AnalysisStatus);
			}
		}

		public bool CanUpdate => _canUpdate;

		public AnalysisStatistics(IProject project, ILanguageDirection languageDirection, TranslatableFile translatableFile, Sdl.ProjectApi.Implementation.Xml.AnalysisStatistics xmlAnalysisStatistics, bool canUpdate)
			: base(project)
		{
			_languageDirection = languageDirection;
			_translatableFile = translatableFile;
			_xmlAnalysisStatistics = xmlAnalysisStatistics;
			_canUpdate = canUpdate;
			EnsureCountDataObjects(base.Project, _xmlAnalysisStatistics);
			_repetitions = (ICountData)(object)new CountDataRepository(_xmlAnalysisStatistics.Repetitions);
			_exact = (ICountData)(object)new CountDataRepository(_xmlAnalysisStatistics.Exact);
			_inContextExact = (ICountData)(object)new CountDataRepository(_xmlAnalysisStatistics.InContextExact);
			_perfect = (ICountData)(object)new CountDataRepository(_xmlAnalysisStatistics.Perfect);
			_total = (ICountData)(object)new CountDataRepository(_xmlAnalysisStatistics.Total);
			_new = (ICountData)(object)new CountDataRepository(_xmlAnalysisStatistics.New);
			EnsureForwardsCompatibility(_xmlAnalysisStatistics);
			_locked = (ICountData)(object)new CountDataRepository(_xmlAnalysisStatistics.Locked);
			IAnalysisBand[] analysisBands = ((IProjectConfiguration)project).AnalysisBands;
			_fuzzyCountData = (IFuzzyCountData[])(object)new IFuzzyCountData[analysisBands.Length];
			for (int i = 0; i < _xmlAnalysisStatistics.Fuzzy.Count; i++)
			{
				_fuzzyCountData[i] = (IFuzzyCountData)(object)new FuzzyCountData(analysisBands[i], _xmlAnalysisStatistics.Fuzzy[i]);
			}
		}

		public void UpdateAll()
		{
			if (!_canUpdate)
			{
				throw new InvalidOperationException("These statistics are read-only.");
			}
			_xmlAnalysisStatistics.WordCountStatus = ValueStatus.Complete;
			_xmlAnalysisStatistics.AnalysisStatus = ValueStatus.Complete;
			if (_translatableFile != null)
			{
				DateTime fileTimeStamp = GetFileTimeStamp();
				_xmlAnalysisStatistics.WordCountFileTimeStampSpecified = true;
				_xmlAnalysisStatistics.WordCountFileTimeStamp = fileTimeStamp;
				_xmlAnalysisStatistics.AnalysisFileTimeStampSpecified = true;
				_xmlAnalysisStatistics.AnalysisFileTimeStamp = fileTimeStamp;
			}
			if (_languageDirection != null)
			{
				base.ProjectImpl.NotifyAnalysisStatisticsChanged(_languageDirection);
			}
			else
			{
				base.ProjectImpl.NotifyWordCountStatisticsChanged();
			}
		}

		public void UpdateWordCountOnly()
		{
			if (!_canUpdate)
			{
				throw new InvalidOperationException("These statistics are read-only.");
			}
			_xmlAnalysisStatistics.WordCountStatus = ValueStatus.Complete;
			if (_translatableFile != null)
			{
				_xmlAnalysisStatistics.WordCountFileTimeStampSpecified = true;
				_xmlAnalysisStatistics.WordCountFileTimeStamp = GetFileTimeStamp();
			}
			base.ProjectImpl.NotifyWordCountStatisticsChanged();
		}

		private static void EnsureForwardsCompatibility(Sdl.ProjectApi.Implementation.Xml.AnalysisStatistics xmlAnalysisStatistics)
		{
			if (xmlAnalysisStatistics.Locked == null)
			{
				xmlAnalysisStatistics.Locked = new Sdl.ProjectApi.Implementation.Xml.CountData();
			}
		}

		private static void EnsureCountDataObjects(IProject project, Sdl.ProjectApi.Implementation.Xml.AnalysisStatistics xmlAnalysisStatistics)
		{
			if (xmlAnalysisStatistics.Exact == null)
			{
				if (xmlAnalysisStatistics.Total == null)
				{
					xmlAnalysisStatistics.WordCountStatus = ValueStatus.None;
					xmlAnalysisStatistics.Total = new Sdl.ProjectApi.Implementation.Xml.CountData();
					xmlAnalysisStatistics.Repetitions = new Sdl.ProjectApi.Implementation.Xml.CountData();
				}
				xmlAnalysisStatistics.AnalysisStatus = ValueStatus.None;
				xmlAnalysisStatistics.Exact = new Sdl.ProjectApi.Implementation.Xml.CountData();
				xmlAnalysisStatistics.InContextExact = new Sdl.ProjectApi.Implementation.Xml.CountData();
				xmlAnalysisStatistics.New = new Sdl.ProjectApi.Implementation.Xml.CountData();
				xmlAnalysisStatistics.Perfect = new Sdl.ProjectApi.Implementation.Xml.CountData();
				xmlAnalysisStatistics.Locked = new Sdl.ProjectApi.Implementation.Xml.CountData();
			}
			if (xmlAnalysisStatistics.Fuzzy.Count == 0)
			{
				for (int i = 0; i < ((IProjectConfiguration)project).AnalysisBands.Length; i++)
				{
					xmlAnalysisStatistics.Fuzzy.Add(new Sdl.ProjectApi.Implementation.Xml.CountData());
				}
			}
		}

		public void Reset()
		{
			EnsureCountDataObjects(base.Project, _xmlAnalysisStatistics);
			_xmlAnalysisStatistics.WordCountStatus = ValueStatus.None;
			_xmlAnalysisStatistics.AnalysisStatus = ValueStatus.None;
			_repetitions.Reset();
			_exact.Reset();
			_inContextExact.Reset();
			_new.Reset();
			_perfect.Reset();
			_total.Reset();
			_locked.Reset();
			IFuzzyCountData[] fuzzyCountData = _fuzzyCountData;
			foreach (IFuzzyCountData val in fuzzyCountData)
			{
				if (val != null)
				{
					((ICountData)val).Reset();
				}
			}
		}

		private DateTime GetFileTimeStamp()
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Invalid comparison between Unknown and I4
			IMergedTranslatableFile mergedFile = _translatableFile.MergedFile;
			string path = ((mergedFile == null || (int)mergedFile.MergeState != 1) ? _translatableFile.LocalFilePath : ((IProjectFile)mergedFile).LocalFilePath);
			return File.GetLastWriteTimeUtc(path);
		}

		public static void ResetAnalysisCounts(IProject project, Sdl.ProjectApi.Implementation.Xml.AnalysisStatistics xmlAnalysisStatistics)
		{
			xmlAnalysisStatistics.AnalysisStatus = ValueStatus.None;
			xmlAnalysisStatistics.Exact.Reset();
			xmlAnalysisStatistics.InContextExact.Reset();
			xmlAnalysisStatistics.New.Reset();
			xmlAnalysisStatistics.Perfect.Reset();
			xmlAnalysisStatistics.Fuzzy.Clear();
			EnsureForwardsCompatibility(xmlAnalysisStatistics);
			xmlAnalysisStatistics.Locked.Reset();
			EnsureCountDataObjects(project, xmlAnalysisStatistics);
		}
	}
}
