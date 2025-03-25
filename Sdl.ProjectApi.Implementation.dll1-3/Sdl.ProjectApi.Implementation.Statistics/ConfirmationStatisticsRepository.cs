using System;
using System.Collections.Generic;
using System.IO;
using Sdl.Core.Globalization;
using Sdl.ProjectApi.Implementation.Xml;

namespace Sdl.ProjectApi.Implementation.Statistics
{
	internal class ConfirmationStatisticsRepository : AbstractConfirmationStatistics, IConfirmationStatistics
	{
		private readonly Sdl.ProjectApi.Implementation.Xml.ConfirmationStatistics _xmlConfirmationStatistics;

		private readonly Dictionary<ConfirmationLevel, ICountData> _stats;

		private readonly bool _canUpdate;

		private readonly ILanguageDirection _languageDirection;

		private readonly TranslatableFile _translatableFile;

		public override ICountData this[ConfirmationLevel confirmationLevel] => _stats[confirmationLevel];

		public override ICountData Total
		{
			get
			{
				CountData countData = new CountData();
				foreach (ICountData value in _stats.Values)
				{
					countData.Increment(value);
				}
				return (ICountData)(object)countData;
			}
		}

		public override ValueStatus Status
		{
			get
			{
				//IL_0057: Unknown result type (might be due to invalid IL or missing references)
				if (_translatableFile != null && _xmlConfirmationStatistics.FileTimeStampSpecified && (_xmlConfirmationStatistics.Status == ValueStatus.Complete || _xmlConfirmationStatistics.Status == ValueStatus.OutOfDate))
				{
					if (_xmlConfirmationStatistics.FileTimeStamp != GetFileTimeStamp())
					{
						return (ValueStatus)2;
					}
					return (ValueStatus)3;
				}
				return EnumConvert.ConvertValueStatus(_xmlConfirmationStatistics.Status);
			}
		}

		public override bool CanUpdate => _canUpdate;

		public ConfirmationStatisticsRepository(ILanguageDirection languageDirection, TranslatableFile translatableFile, Sdl.ProjectApi.Implementation.Xml.ConfirmationStatistics xmlConfirmationStatistics, bool canUpdate)
		{
			_languageDirection = languageDirection;
			_translatableFile = translatableFile;
			_xmlConfirmationStatistics = xmlConfirmationStatistics;
			_canUpdate = canUpdate;
			EnsureCountDataObjects();
			_stats = new Dictionary<ConfirmationLevel, ICountData>(6);
			AddLevel((ConfirmationLevel)0, _xmlConfirmationStatistics.Unspecified);
			AddLevel((ConfirmationLevel)1, _xmlConfirmationStatistics.Draft);
			AddLevel((ConfirmationLevel)2, _xmlConfirmationStatistics.Translated);
			AddLevel((ConfirmationLevel)3, _xmlConfirmationStatistics.RejectedTranslation);
			AddLevel((ConfirmationLevel)4, _xmlConfirmationStatistics.ApprovedTranslation);
			AddLevel((ConfirmationLevel)5, _xmlConfirmationStatistics.RejectedSignOff);
			AddLevel((ConfirmationLevel)6, _xmlConfirmationStatistics.ApprovedSignOff);
		}

		private void AddLevel(ConfirmationLevel level, Sdl.ProjectApi.Implementation.Xml.CountData data)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_stats[level] = (ICountData)(object)new CountDataRepository(data);
		}

		private void EnsureCountDataObjects()
		{
			if (_xmlConfirmationStatistics.Draft == null)
			{
				_xmlConfirmationStatistics.Unspecified = new Sdl.ProjectApi.Implementation.Xml.CountData();
				_xmlConfirmationStatistics.Draft = new Sdl.ProjectApi.Implementation.Xml.CountData();
				_xmlConfirmationStatistics.Translated = new Sdl.ProjectApi.Implementation.Xml.CountData();
				_xmlConfirmationStatistics.RejectedTranslation = new Sdl.ProjectApi.Implementation.Xml.CountData();
				_xmlConfirmationStatistics.ApprovedTranslation = new Sdl.ProjectApi.Implementation.Xml.CountData();
				_xmlConfirmationStatistics.RejectedSignOff = new Sdl.ProjectApi.Implementation.Xml.CountData();
				_xmlConfirmationStatistics.ApprovedSignOff = new Sdl.ProjectApi.Implementation.Xml.CountData();
				_xmlConfirmationStatistics.Status = ValueStatus.None;
			}
		}

		public override void Update()
		{
			((AbstractConfirmationStatistics)this).Update();
			_xmlConfirmationStatistics.Status = ValueStatus.Complete;
			if (_translatableFile != null)
			{
				_xmlConfirmationStatistics.FileTimeStampSpecified = true;
				_xmlConfirmationStatistics.FileTimeStamp = GetFileTimeStamp();
			}
			((LanguageDirection)(object)_languageDirection).NotifyConfirmationLevelStatisticsChanged();
		}

		private DateTime GetFileTimeStamp()
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I4
			string text = null;
			IMergedTranslatableFile mergedFile = _translatableFile.MergedFile;
			text = ((mergedFile == null || (int)mergedFile.MergeState != 1) ? _translatableFile.LocalFilePath : ((IProjectFile)mergedFile).LocalFilePath);
			return File.GetLastWriteTimeUtc(text);
		}
	}
}
