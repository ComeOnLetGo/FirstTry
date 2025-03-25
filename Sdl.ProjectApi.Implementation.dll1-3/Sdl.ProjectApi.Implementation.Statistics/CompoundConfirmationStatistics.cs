using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sdl.Core.Globalization;

namespace Sdl.ProjectApi.Implementation.Statistics
{
	public class CompoundConfirmationStatistics : AbstractConfirmationStatistics, IConfirmationStatistics
	{
		private readonly Dictionary<ConfirmationLevel, ICountData> _stats;

		private readonly ValueStatus _status;

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

		public override ValueStatus Status => _status;

		public override bool CanUpdate => false;

		private ConfirmationLevel[] ConfirmationLevels
		{
			get
			{
				ConfirmationLevel[] array = new ConfirmationLevel[7];
				RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
				return (ConfirmationLevel[])(object)array;
			}
		}

		public CompoundConfirmationStatistics(IEnumerable<IConfirmationStatistics> statistics)
		{
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0159: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			_stats = new Dictionary<ConfirmationLevel, ICountData>(6);
			_stats.Add((ConfirmationLevel)0, (ICountData)(object)new CountData());
			_stats.Add((ConfirmationLevel)1, (ICountData)(object)new CountData());
			_stats.Add((ConfirmationLevel)2, (ICountData)(object)new CountData());
			_stats.Add((ConfirmationLevel)3, (ICountData)(object)new CountData());
			_stats.Add((ConfirmationLevel)4, (ICountData)(object)new CountData());
			_stats.Add((ConfirmationLevel)5, (ICountData)(object)new CountData());
			_stats.Add((ConfirmationLevel)6, (ICountData)(object)new CountData());
			bool flag = false;
			_status = (ValueStatus)3;
			foreach (IConfirmationStatistics statistic in statistics)
			{
				ValueStatus status = statistic.Status;
				_status = _status.CombineValueStatus(status);
				if ((int)status != 0)
				{
					flag = true;
				}
				ConfirmationLevel[] confirmationLevels = ConfirmationLevels;
				foreach (ConfirmationLevel val in confirmationLevels)
				{
					ICountData val2 = statistic[val];
					ICountData val3 = _stats[val];
					val3.Characters += val2.Characters;
					val3.Words += val2.Words;
					val3.Segments += val2.Segments;
				}
			}
			if (!flag)
			{
				_status = (ValueStatus)0;
			}
		}

		public override void Update()
		{
		}
	}
}
