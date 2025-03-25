using System.Collections.Generic;
using Sdl.Core.Globalization;

namespace Sdl.ProjectApi.Implementation.Statistics
{
	public class ConfirmationStatistics : AbstractConfirmationStatistics, IConfirmationStatistics
	{
		private readonly Dictionary<ConfirmationLevel, ICountData> _stats;

		private ValueStatus _status;

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

		public override bool CanUpdate => true;

		public ConfirmationStatistics()
		{
			_stats = new Dictionary<ConfirmationLevel, ICountData>(7);
			AddLevel((ConfirmationLevel)0, new CountData());
			AddLevel((ConfirmationLevel)1, new CountData());
			AddLevel((ConfirmationLevel)2, new CountData());
			AddLevel((ConfirmationLevel)3, new CountData());
			AddLevel((ConfirmationLevel)4, new CountData());
			AddLevel((ConfirmationLevel)5, new CountData());
			AddLevel((ConfirmationLevel)6, new CountData());
		}

		public ConfirmationStatistics(IConfirmationStatistics other)
			: this()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			_stats = new Dictionary<ConfirmationLevel, ICountData>(7);
			_status = other.Status;
			AddLevel((ConfirmationLevel)6, new CountData(other[(ConfirmationLevel)6]));
			AddLevel((ConfirmationLevel)4, new CountData(other[(ConfirmationLevel)4]));
			AddLevel((ConfirmationLevel)1, new CountData(other[(ConfirmationLevel)1]));
			AddLevel((ConfirmationLevel)5, new CountData(other[(ConfirmationLevel)5]));
			AddLevel((ConfirmationLevel)3, new CountData(other[(ConfirmationLevel)3]));
			AddLevel((ConfirmationLevel)2, new CountData(other[(ConfirmationLevel)2]));
			AddLevel((ConfirmationLevel)0, new CountData(other[(ConfirmationLevel)0]));
		}

		private void AddLevel(ConfirmationLevel level, CountData data)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			_stats[level] = (ICountData)(object)data;
		}

		public override void Update()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			((AbstractConfirmationStatistics)this).Update();
			_status = (ValueStatus)3;
		}
	}
}
